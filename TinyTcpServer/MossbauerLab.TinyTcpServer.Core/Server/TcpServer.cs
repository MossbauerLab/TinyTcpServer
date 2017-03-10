using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MossbauerLab.TinyTcpServer.Core.Client;
using MossbauerLab.TinyTcpServer.Core.Handlers;
using MossbauerLab.TinyTcpServer.Core.Handlers.Utils;

namespace MossbauerLab.TinyTcpServer.Core.Server
{
    public class TcpServer : ITcpServer, IDisposable
    {
        public TcpServer(String ipAddress = DefaultServerIpAddress, UInt16 port = DefaultServerPort)
        {
            AssignIpAddressAndPort(ipAddress, port);
            _clientProcessingTasks = new Task[_parallelClientProcessingTasks];
            _clientConnectingTask = new Task(ClientConnectProcessing, new CancellationToken(_interruptRequested));
        }

        public Boolean Start(String ipAddress, UInt16 port)
        {
            try
            {
                _interruptRequested = false;
                AssignIpAddressAndPort(ipAddress, port);
                _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                _tcpListener.Server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, true);
                _tcpListener.Start();
                Task.Factory.StartNew(StartClientProcessing);
                return _tcpListener.Server.IsBound;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Stop(Boolean clearHandlers)
        {
            _interruptRequested = true;
            _tcpListener.Stop();
            _tcpListener.Server.Close(ServerCloseTimeout);
            if (clearHandlers)
                ReleaseClientsHandlers();
        }

        public void Restart()
        {
            Stop(false);
            Start(_ipAddress, _port);
        }

        public void Dispose()
        {
            _clientConnectingTask.Dispose();
            foreach (Task clientProcessingTask in _clientProcessingTasks)
                if(clientProcessingTask != null)
                    clientProcessingTask.Dispose();
            _clientConnectEvent.Dispose();
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

        public void SendData(TcpClientHandlerInfo clientHandlerInfo, Byte[] data)
        {
            IList<TcpClientContext> selectedClients = _tcpClients.Where(item =>
            {
                return TcpClientHandlerSelector.Select(clientHandlerInfo, item);
            }).ToList();

            for (Int32 clientCounter = 0; clientCounter < selectedClients.Count; clientCounter++)
            {
                TcpClientContext client = selectedClients[clientCounter];
                Task.Factory.StartNew(() => SendImpl(client, data));
            }
        }

        public Int32 ConnectedClients
        {
            get { return _tcpClients.Count; }
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
                _tcpListener.Server.Close(ServerCloseTimeout);
                _tcpListener.Server.Dispose();
                _tcpListener.Stop();
            }
            _tcpListener = null;
            _tcpListener = new TcpListener(IPAddress.Parse(_ipAddress), _port);
        }

        private void ReleaseClients()
        {
            foreach (TcpClientContext tcpClient in _tcpClients)
            {
                tcpClient.Client.GetStream().Flush();
                tcpClient.Client.GetStream().Close();
                tcpClient.Client.Client.Close();
            }
            _tcpClients.Clear();
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
                                    //Console.WriteLine("[Server, StartClientProcessing] Starting task 4 IO with client");
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
                            else if (client.InactiveTimeMark.AddSeconds(DefaultClientInactiveWaitSeconds) < DateTime.Now)
                            {
                                client.ReadDataEvent.Dispose();
                                client.WriteDataEvent.Dispose();
                                _tcpClients.Remove(client);
                            }
                        }
                    }
                }
            }
        }

        private void ClientConnectProcessing()
        {
            //Console.WriteLine("[Server, ClientConnectProcessing]waiting 4 clients");
            Int32 clientsNumber = _tcpClients.Count;
            for (Int32 attempt = 0; attempt < _clientConnectAttempts; attempt++)
            {
                _clientConnectEvent.Reset();
                _tcpListener.BeginAcceptTcpClient(ConnectAsyncCallback, _tcpListener);
                _clientConnectEvent.Wait(_clientConnectTimeout);
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
                    lock(_tcpClients)
                        _tcpClients.Add(new TcpClientContext(client));
                }
            }
            catch (Exception)
            {
                //todo: umv: probably we should notify i.e. via logs
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
                return false;
            }
        }

        private void ProcessClientReceiveSend(TcpClientContext client)
        {
            //Console.WriteLine("[Server ProcessClientReceiveSend] IO with client");
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
                //Console.WriteLine("[Server ProcessClientReceiveSend] found {0} handlers", linkedHandlers.Count);
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
            Byte[] buffer = new Byte[DefaultClientBufferSize];
            client.BytesRead = 0;

            try
            {
                //Console.WriteLine("[Server ReceiveImpl] waiting 4 data");
                for (Int32 attempt = 0; attempt < _clientReadAttempts; attempt++)
                {
                    NetworkStream netStream = client.Client.GetStream();
                    Boolean result = netStream.DataAvailable;
                    while (result)
                    {
                        client.ReadDataEvent.Reset();
                        if (buffer.Length < client.BytesRead + DefaultChunkSize)
                            Array.Resize(ref buffer, buffer.Length + 10 * DefaultChunkSize);
                        Int32 offset = client.BytesRead;
                        Int32 size = DefaultChunkSize;
                        //lock (client.SynchObject)
                        netStream.BeginRead(buffer, offset, size, ReadAsyncCallback, client);
                        client.ReadDataEvent.Wait(_readTimeout);
                        result = netStream.DataAvailable;
                    }
                    client.Client.Client.Poll(_pollTime,  SelectMode.SelectRead);
                }
                Array.Resize(ref buffer, client.BytesRead);
                //Console.WriteLine("[SERVER, ReceiveImpl] Read bytes: " + client.BytesRead);
            }
            catch (Exception)
            {
                // todo: umv: add exception handling ....
                buffer = null;
                //Console.WriteLine("Something goes wrong [read]");
            }

            return buffer;
        }

        private void ReadAsyncCallback(IAsyncResult state)
        {
            TcpClientContext client = state.AsyncState as TcpClientContext;
            if(client == null)
                throw new ApplicationException("state can't be null");
            client.BytesRead +=client.Client.GetStream().EndRead(state);
            client.ReadDataEvent.Set();
        }

        private void SendImpl(TcpClientContext client, Byte[] data)
        {
            try
            {
                //client.Client.Client.Poll(100, SelectMode.SelectWrite);
                //Console.WriteLine("[Server, SendImpl] Write started");
                lock (client.WriteDataEvent)
                {
                    client.WriteDataEvent.Reset();
                    NetworkStream netStream = client.Client.GetStream();
                    //lock (client.SynchObject)
                    netStream.BeginWrite(data, 0, data.Length, WriteAsyncCallback, client);
                    client.WriteDataEvent.Wait(_writeTimeout);
                }
                //client.Client.Client.Poll(_pollTime, SelectMode.SelectWrite);
                //client.Client.Client.Poll(_pollTime, SelectMode.SelectError);
                //netStream.FlushAsync(); // net 4.5
                //Console.WriteLine("[Server, SendImpl] Write done");
            }
            catch (Exception)
            {
                //todo: umv: add error handling
                //Console.WriteLine("[Server, SendImpl] Something goes wrong");
            }
        }

        private void WriteAsyncCallback(IAsyncResult state)
        {
            TcpClientContext client = state.AsyncState as TcpClientContext;
            if (client == null)
                throw new ApplicationException("state can't be null");
            client.Client.GetStream().EndWrite(state);
            //Console.WriteLine("[Server, WriteAsyncCallback] Write done!");
            client.WriteDataEvent.Set();
        }

        private const String DefaultServerIpAddress = "127.0.0.1";
        private const Int32 DefaultServerPort = 16000;
        private const Int32 ServerCloseTimeout = 2000;
        private const Int32 DefaultClientBufferSize = 16384;
        private const Int32 DefaultChunkSize = 1536;
        private const Int32 DefaultClientConnectAttempts = 1;
        private const Int32 DefaultMaximumClientConnectTimeout = 200;    //ms
        private const Int32 DefaultMaximumReadTimeout = 1000;            //ms
        private const Int32 DefaultMaximumWriteTimeout = 1000;           //ms
        private const Int32 DefaultPollTime = 1;                         //us
        private const Int32 DefaultReadAttempts = 2;
        private const Int32 DefaultParallelClientProcessingTasks = 32;
        private const Int32 DefaultClientInactiveWaitSeconds = 45;

        // timeouts
        //todo: umv: make adjustable
        private Int32 _clientConnectTimeout = DefaultMaximumClientConnectTimeout;
        private Int32 _readTimeout = DefaultMaximumReadTimeout;
        private Int32 _writeTimeout = DefaultMaximumWriteTimeout;
        private Int32 _pollTime = DefaultPollTime;
        // other parameters
        private Int32 _clientConnectAttempts = DefaultClientConnectAttempts;
        private Int32 _clientReadAttempts = DefaultReadAttempts;
        private Int32 _parallelClientProcessingTasks = DefaultParallelClientProcessingTasks;

        // threading things
        private readonly ManualResetEventSlim _clientConnectEvent = new ManualResetEventSlim(false, 100);
        private readonly IList<Task> _clientProcessingTasks;
        private Task _clientConnectingTask;                                                           
        
        private readonly IList<Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>>>  _clientsHandlers = new List<Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>>>();
        private readonly IList<TcpClientContext> _tcpClients = new List<TcpClientContext>(); 
        private String _ipAddress;
        private UInt16 _port;
        private TcpListener _tcpListener;
        private Boolean _interruptRequested;
    }
}
