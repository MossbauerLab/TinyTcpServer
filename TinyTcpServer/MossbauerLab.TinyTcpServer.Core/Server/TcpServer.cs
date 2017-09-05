using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using MossbauerLab.TinyTcpServer.Core.Client;
using MossbauerLab.TinyTcpServer.Core.Handlers;
using MossbauerLab.TinyTcpServer.Core.Handlers.Utils;

namespace MossbauerLab.TinyTcpServer.Core.Server
{
    public class TcpServer : ITcpServer, IDisposable
    {
        public TcpServer(String ipAddress = DefaultServerIpAddress, UInt16 port = DefaultServerPort, ILog logger = null, 
                         Boolean debug = false, TcpServerConfig config = null)
        {
            _config = config ?? new TcpServerConfig();
            AssignIpAddressAndPort(ipAddress, port);
            _clientProcessingTasks = new Task[_config.ParallelTask];
            _clientConnectingTask = new Task(ClientConnectProcessing, new CancellationToken(_interruptRequested));
            if (logger != null)
                _logger = logger;
            else
            {
                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
                Logger loggerImpl = hierarchy.LoggerFactory.CreateLogger(hierarchy, "Logger4Tests");
                loggerImpl.Hierarchy = hierarchy;
                loggerImpl.AddAppender(new RollingFileAppender());
                loggerImpl.AddAppender(new ColoredConsoleAppender());
                loggerImpl.Repository.Configured = true;

                hierarchy.Threshold = debug ? Level.Debug : Level.Info;
                loggerImpl.Level = Level.All;

                _logger = new LogImpl(loggerImpl);
            }
            if(debug)
                _logger.Debug("Server was inited in DEBUG mode");
        }

        public virtual Boolean Start()
        {
            return StartImpl(false, _ipAddress, _port);
        }

        public virtual Boolean Start(String ipAddress, UInt16 port)
        {
            return StartImpl(true, ipAddress, port);
        }

        public virtual void Stop(Boolean clearHandlers)
        {
            _interruptRequested = true;
            _tcpListener.Stop();
            _tcpListener.Server.Close(_config.ServerCloseTimeout);
            if (clearHandlers)
                ReleaseClientsHandlers();
            if (_clientConnectEvent != null)
                _clientConnectEvent.Dispose();
            _serverStartedProcessing = false;
            _logger.Info(ServerStoppedTemplate);
        }

        public virtual void Restart()
        {
            Stop(false);
            Start();
        }

        public virtual void Restart(String ipAddress, UInt16 port)
        {
            Stop(false);
            Start(ipAddress, port);
        }

        public void Dispose()
        {
            _clientConnectingTask.Dispose();
            foreach (Task clientProcessingTask in _clientProcessingTasks)
                if(clientProcessingTask != null)
                    clientProcessingTask.Dispose();
            if (_clientConnectEvent != null)
                _clientConnectEvent.Dispose();
        }

        public Boolean IsReady 
        {
            get
            {
                if (_tcpListener == null)
                    return false;
                return _tcpListener.Server.IsBound && _serverStartedProcessing;
            }
        }

        public void DisconnectAllClients()
        {
            ReleaseClients();
        }

        public void AddHandler(TcpClientHandlerInfo clientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]> handler)
        {
            if(clientHandlerInfo == null)
                throw new ArgumentNullException("clientHandlerInfo");
            if(handler == null)
                throw new ArgumentNullException("handler");
            Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>> existingHandler =_clientsHandlers.FirstOrDefault(item => item.Item1.Id.Equals(clientHandlerInfo.Id));
            if (existingHandler == null)
                _clientsHandlers.Add(new Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>>(clientHandlerInfo, handler));
        }

        public void RemoveHandler(TcpClientHandlerInfo clientHandlerInfo)
        {
            Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>> handler =_clientsHandlers.FirstOrDefault(item => item.Item1.Id.Equals(clientHandlerInfo.Id));
            if (handler != null)
                _clientsHandlers.Remove(handler);
        }

        public void AddConnectionHandler(Guid id, Action<TcpClientContext, Boolean> handler)
        {
            if(handler == null)
                throw new ArgumentNullException("handler");
            if (!_connectHandlers.ContainsKey(id))
                _connectHandlers.Add(id, handler);
        }

        public void RemoveConnectionHandler(Guid id)
        {
            if (_connectHandlers.ContainsKey(id))
                _connectHandlers.Remove(id);
        }

        public void SendData(TcpClientHandlerInfo clientHandlerInfo, Byte[] data)
        {
            IList<TcpClientContext> selectedClients;
            lock (_tcpClients)
            {
                selectedClients = _tcpClients.Where(item => TcpClientHandlerSelector.Select(clientHandlerInfo, item)).ToList();
            }

            foreach (TcpClientContext client in selectedClients)
            {
                TcpClientContext clientCopy = client;
                Task.Factory.StartNew(() => SendImpl(clientCopy, data));
            }
        }

        public Int32 ConnectedClients
        {
            get { return _tcpClients.Count; }
        }

        public IList<TcpClientContext> Clients
        {
            get { return _tcpClients; }
        }

        private Boolean StartImpl(Boolean assignNewValues, String ipAddress, UInt16 port)
        {
            try
            {
                _serverStartedProcessing = false;
                _clientConnectEvent = new ManualResetEventSlim(false, 100);
                _interruptRequested = false;
                if(assignNewValues)
                    AssignIpAddressAndPort(ipAddress, port);
                else
                {
                    _tcpListener = null;
                    _tcpListener = new TcpListener(IPAddress.Parse(_ipAddress), _port);
                }
                _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
                _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                _tcpListener.Server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, true);
                _tcpListener.Start();
                Task.Factory.StartNew(StartClientProcessing);
                //_serverMainTask = new Task(StartClientProcessing);
                //_serverMainTask.Start();
                _logger.InfoFormat(ServerStarteTemplated, _ipAddress, _port);
                return _tcpListener.Server.IsBound;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void AssignIpAddressAndPort(String ipAddress, UInt16 port)
        {
            if(String.IsNullOrEmpty(ipAddress))
                throw new ArgumentNullException("ipAddress");
            _ipAddress = ipAddress;
            _port = port;
            if (_tcpListener != null)
            {
                ReleaseClients();
                _tcpListener.Server.Close(_config.ServerCloseTimeout);
                _tcpListener.Server.Dispose();
                _tcpListener.Stop();
            }
            _tcpListener = null;
            _tcpListener = new TcpListener(IPAddress.Parse(_ipAddress), _port);
        }

        private void ReleaseClients()
        {
            lock (_tcpClients)
            {
                foreach (TcpClientContext tcpClient in _tcpClients)
                {
                    tcpClient.Client.GetStream().Flush();
                    tcpClient.Client.GetStream().Close();
                    tcpClient.Client.Client.Close();
                }
                _tcpClients.Clear();
                _logger.Debug("All clients were closed and resources were released");
            }
        }

        private void ReleaseClientsHandlers()
        {
            _clientsHandlers.Clear();
        }

        private void StartClientProcessing()
        {
            Int32 clientIndex = 0;
            while (!_interruptRequested)
            {
                // 1. waiting for connection ...
                if (_clientConnectingTask.IsCompleted || 
                    !(_clientConnectingTask.Status == TaskStatus.Running || _clientConnectingTask.Status == TaskStatus.WaitingToRun || 
                      _clientConnectingTask.Status == TaskStatus.WaitingForActivation || _clientConnectingTask.Status == TaskStatus.WaitingForChildrenToComplete))
                {
                    _clientConnectingTask = new Task(ClientConnectProcessing, new CancellationToken(_interruptRequested));
                    _clientConnectingTask.Start();
                }

                /*if (_tcpClients.Count == 0)
                {
                }*/

                if(_tcpClients.Count != 0)
                { 
                    // 2. handle clients ... (read + write)
                    lock (_tcpClients)
                    {
                        if (clientIndex >= _tcpClients.Count)
                            clientIndex = 0;
                        for (Int32 clientCounter = clientIndex; clientCounter < _tcpClients.Count; clientCounter++)
                        {
                            if (CheckClientConnected(_tcpClients[clientCounter].Client) && !_tcpClients[clientCounter].IsProcessing)
                            {
                                TcpClientContext client = _tcpClients[clientCounter];
                                Int32 freeTaskIndex = -1;
                                for (Int32 taskCounter = 0; taskCounter < _clientProcessingTasks.Count; taskCounter++)
                                {
                                    if (_clientProcessingTasks[taskCounter] == null ||
                                        _clientProcessingTasks[taskCounter].IsCompleted ||
                                        (_clientProcessingTasks[taskCounter].Status != TaskStatus.Running &&
                                         _clientProcessingTasks[taskCounter].Status != TaskStatus.WaitingToRun &&
                                         _clientProcessingTasks[taskCounter].Status != TaskStatus.WaitingForActivation &&
                                         _clientProcessingTasks[taskCounter].Status != TaskStatus.WaitingForChildrenToComplete))
                                    {
                                        freeTaskIndex = taskCounter;
                                        break;
                                    }
                                }
                                if (freeTaskIndex >= 0)
                                {
                                    _tcpClients[clientCounter].IsProcessing = true;
                                    _clientProcessingTasks[freeTaskIndex] = new Task(() => ProcessClientReceiveSend(client), new CancellationToken(_interruptRequested));
                                    _clientProcessingTasks[freeTaskIndex].Start();
                                }
                                else clientIndex = clientCounter;
                            }
                        }
                    }
                
                    // 3. check "disconnected" clients ...
                    lock (_tcpClients)
                    {
                        IList<TcpClientContext> disoonnectedClients = _tcpClients.Where(client => !client.IsProcessing && !CheckClientConnected(client.Client)).ToList();
                        //todo: umv mark disconnected and check activity during some time ....
                        foreach (TcpClientContext client in disoonnectedClients)
                        {
                            
                            client.Inactive = true;
                            if (client.InactiveTimeMark == default(DateTime))
                                client.InactiveTimeMark = DateTime.Now;
                            else if (client.InactiveTimeMark.AddSeconds(_config.ClientInactivityTime) < DateTime.Now)
                            {
                                client.ReadDataEvent.Dispose();
                                client.WriteDataEvent.Dispose();
                                _logger.DebugFormat(ClientRemoveMessagedTemplate, client.Id, ((IPEndPoint)client.Client.Client.LocalEndPoint).Address);
                                foreach (KeyValuePair<Guid, Action<TcpClientContext, Boolean>> handler in _connectHandlers)
                                    handler.Value(client, false);
                                _tcpClients.Remove(client);
                            }
                        }
                    }
                }
            }
        }

        private void ClientConnectProcessing()
        {
            _serverStartedProcessing = true;
            Int32 clientsNumber = _tcpClients.Count;
            for (Int32 attempt = 0; attempt < _config.ClientConnectAttempts; attempt++)
            {
                if (_interruptRequested)
                    return;
                _clientConnectEvent.Reset();
                _tcpListener.BeginAcceptTcpClient(ConnectAsyncCallback, _tcpListener);
                _clientConnectEvent.Wait(_config.ClientConnectTimeout);
                if (_tcpClients.Count > clientsNumber)
                    break;
            }
        }

        private void ConnectAsyncCallback(IAsyncResult state)
        {
            try
            {
                TcpClient client = _tcpListener.EndAcceptTcpClient(state);
                if (client.Connected)
                {
                    client.NoDelay = true;
                    TcpClientContext clientContext = new TcpClientContext(client);
                    lock(_tcpClients)
                        _tcpClients.Add(clientContext);
                    foreach (KeyValuePair<Guid, Action<TcpClientContext, Boolean>> handler in _connectHandlers)
                    {
                        handler.Value(clientContext, true);
                    }
                    _logger.DebugFormat(ClientConnectedMessagedTemplate, clientContext.Id, ((IPEndPoint)client.Client.LocalEndPoint).Address);
                }
            }
            catch (Exception)
            {
                if (!_interruptRequested)
                    _logger.Error("An error occured during client connection");
            }
            _clientConnectEvent.Set();
        }

        private Boolean CheckClientConnected(TcpClient client, Boolean formalCheck = false)
        {
            if (formalCheck)
                return client != null && client.Connected;
            if (client == null || !client.Connected)
                return false;
            try
            {
                return !(client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0);
            }
            catch (Exception)
            {
                _logger.Error("An error occured during check client state");
                return false;
            }
        }

        private void ProcessClientReceiveSend(TcpClientContext client)
        {
            Byte[] receivedData = ReceiveImpl(client);
            client.Inactive = false;
            client.InactiveTimeMark = default(DateTime);
            if (receivedData != null)
            {
                IList<Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>>> linkedHandlers =
                _clientsHandlers.Where(item =>
                {
                    //todo: umv: add special selection for AnyPort and AnyIp
                    return TcpClientHandlerSelector.Select(item.Item1, client);
                }).ToList();
                foreach (Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>> handler in linkedHandlers)
                {
                    Byte[] dataForSend = handler.Item2(receivedData, handler.Item1);
                    if (dataForSend != null && dataForSend.Length > 0)
                        SendImpl(client, dataForSend);
                }
            }
            client.IsProcessing = false;
        }

        private Byte[] ReceiveImpl(TcpClientContext client)
        {
            Byte[] buffer = new Byte[_config.ClientBufferSize];
            client.BytesRead = 0;

            try
            {
                NetworkStream netStream = client.Client.GetStream();
                netStream.ReadTimeout = _config.ReadTimeout;
                for (Int32 attempt = 0; attempt < _config.ClientReadAttempts; attempt++)
                {
                    Boolean result = netStream.DataAvailable;
                    while (result)
                    {
                        client.ReadDataEvent.Reset();
                        Array.Resize(ref buffer, buffer.Length + _config.ChunkSize);
                        Int32 offset = client.BytesRead;
                        Int32 size = _config.ChunkSize;
                        netStream.BeginRead(buffer, offset, size, ReadAsyncCallback, client);
                        client.ReadDataEvent.Wait(_config.ReadTimeout);
                        result = netStream.DataAvailable;
                    }
                }
                Array.Resize(ref buffer, client.BytesRead);
                if(client.BytesRead > 0)
                    _logger.DebugFormat(ReceivedDataMessageTemplate, client.BytesRead, client.Id, ((IPEndPoint)client.Client.Client.LocalEndPoint).Address);
            }
            catch (Exception)
            {
                // todo: umv: add exception handling ....
                buffer = null;
                _logger.Error("Error occured during data read");
            }

            return buffer;
        }

        private void ReadAsyncCallback(IAsyncResult state)
        {
            TcpClientContext client = state.AsyncState as TcpClientContext;
            if(client == null)
                throw new ApplicationException("state can't be null");
            client.BytesRead += client.Client.GetStream().EndRead(state);
            client.ReadDataEvent.Set();
        }

        private void SendImpl(TcpClientContext client, Byte[] data)
        {
            try
            {
                _logger.DebugFormat(SendDataMessageTemplate, data.Length, client.Id, ((IPEndPoint)client.Client.Client.LocalEndPoint).Address);
                lock (client.WriteDataEvent)
                {
                    
                    client.WriteDataEvent.Reset();
                    NetworkStream netStream = client.Client.GetStream();
                    netStream.WriteTimeout = _config.WriteTimeout;
                    netStream.BeginWrite(data, 0, data.Length, WriteAsyncCallback, client);
                    client.WriteDataEvent.Wait(_config.WriteTimeout);
                }
            }
            catch (Exception)
            {
                //todo: umv: add error handling
                _logger.Error("An error occured during send data to client");
            }
        }

        private void WriteAsyncCallback(IAsyncResult state)
        {
            TcpClientContext client = state.AsyncState as TcpClientContext;
            if (client == null)
                throw new ApplicationException("state can't be null");
            client.Client.GetStream().EndWrite(state);
            client.WriteDataEvent.Set();
        }

        private const String DefaultServerIpAddress = "127.0.0.1";
        private const Int32 DefaultServerPort = 16000;
        private const String ClientRemoveMessagedTemplate = "Client {0} connected from {1} ip was removed due to no activity";
        private const String ClientConnectedMessagedTemplate = "Client {0} connected from {1} ip address";
        private const String ReceivedDataMessageTemplate = "Received {0} bytes from client {1} {2}";
        private const String SendDataMessageTemplate = "There are {0} bytes was sent to client {1} {2}";
        private const String ServerStarteTemplated = "TCP Server successfully started with ip address {0} on {1} port";
        private const String ServerStoppedTemplate = "TCP Server was stopped";

        private readonly TcpServerConfig _config;

        // threading things
        private ManualResetEventSlim _clientConnectEvent;
        private readonly IList<Task> _clientProcessingTasks;
        private Task _clientConnectingTask;
        private Boolean _serverStartedProcessing;
        // private Task _serverMainTask;
        // server and client entities
        private String _ipAddress;
        private UInt16 _port;
        private TcpListener _tcpListener;
        private readonly ILog _logger;
        private readonly IList<Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>>>  _clientsHandlers = new List<Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>>>();
        private readonly IDictionary<Guid, Action<TcpClientContext, Boolean>> _connectHandlers = new Dictionary<Guid, Action<TcpClientContext, Boolean>>();
        private readonly IList<TcpClientContext> _tcpClients = new List<TcpClientContext>(); 
        private Boolean _interruptRequested;
    }
}
