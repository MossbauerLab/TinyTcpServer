using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TinyTcpServer.Core.Client;
using TinyTcpServer.Core.Handlers;
using TinyTcpServer.Core.Handlers.Utils;

namespace TinyTcpServer.Core.Server
{
    public class TcpServer : ITcpServer
    {
        public TcpServer(String ipAddress = DefaultServerIpAddress, UInt16 port = DefaultServerPort)
        {
            AssignIpAddressAndPort(ipAddress, port);
        }

        public Boolean Start(String ipAddress, UInt16 port)
        {
            try
            {
                _interruptRequested = false;
                AssignIpAddressAndPort(ipAddress, port);
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
            while (!_interruptRequested)
            {
                // 1. waiting for connection ...
                Task.Factory.StartNew(ClientConnectProcessing, new CancellationToken(_interruptRequested)).Wait();
                // 2. handle clients ... (read + write)
                lock (_tcpClients)
                {
                    for (Int32 clientCounter = 0; clientCounter < _tcpClients.Count; clientCounter++)
                    {
                        if (CheckClientConnected(_tcpClients[clientCounter].Client) && !_tcpClients[clientCounter].IsProcessing)
                        {
                            _tcpClients[clientCounter].IsProcessing = true;
                            TcpClientContext client = _tcpClients[clientCounter];
                            Task.Factory.StartNew(() => ProcessClientReceiveSend(client), new CancellationToken(_interruptRequested));
                        }
                    }
                }
                // 3. check "disconnected" clients ...
                IList<TcpClientContext> disoonnectedClients = _tcpClients.Where(client => !CheckClientConnected(client.Client)).ToList();
                foreach (TcpClientContext client in disoonnectedClients)
                    _tcpClients.Remove(client);
            }
        }

        private void ClientConnectProcessing()
        {
            Console.WriteLine("waiting 4 clients");
            for (Int32 attempt = 0; attempt < _clientConnectAttempts; attempt++)
            {
                _clientConnectEvent.Reset();
                lock (_synch)
                    _tcpListener.BeginAcceptTcpClient(ConnectAsyncCallback, _tcpListener);
                _clientConnectEvent.Wait(_clientConnectTimeout);
            }
        }

        private void ConnectAsyncCallback(IAsyncResult state)
        {
            try
            {
                TcpClient client = _tcpListener.EndAcceptTcpClient(state);
                //client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                if(client.Connected)
                    _tcpClients.Add(new TcpClientContext(client));
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
                if (client.Client.Poll(0, SelectMode.SelectWrite) && !client.Client.Poll(0, SelectMode.SelectError))
                {
                    Byte[] buffer = new Byte[1];
                    return client.Client.Receive(buffer, SocketFlags.Peek) != 0;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ProcessClientReceiveSend(TcpClientContext client)
        {
            Byte[] receivedData = ReceiveImpl(client);
            if (receivedData != null)
            {
                IList<Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>>>  linkedHandlers =
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
            Byte[] buffer = new Byte[DefaultClientBufferSize];
            client.BytesRead = 0;
            
            try
            {
                lock(client.SynchObject)
                { 
                    NetworkStream netStream = client.Client.GetStream();
                    netStream.ReadTimeout = 10;//100;//1500;
                    while (netStream.DataAvailable || client.Client.Client.Poll(20000, SelectMode.SelectRead))
                    {
                        client.ReadDataEvent.Reset();
                        if (buffer.Length < client.BytesRead + DefaultChunkSize)
                            Array.Resize(ref buffer, buffer.Length + 10 * DefaultChunkSize);
                        Int32 offset = client.BytesRead;
                        Int32 size = DefaultChunkSize;
                        netStream.BeginRead(buffer, offset, size, ReadAsyncCallback, client);
                        client.ReadDataEvent.Wait(_readTimeout);
                    }
                    Array.Resize(ref buffer, client.BytesRead);
                    Console.WriteLine("[SERVER, ReceiveImpl] Read bytes: " + client.BytesRead);
                }
            }
            catch (Exception)
            {
                // todo: umv: add exception handling ....
                buffer = null;
                Console.WriteLine("Something goes wrong [read]");
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
                lock (client.SynchObject)
                {
                    client.WriteDataEvent.Reset();
                    NetworkStream netStream = client.Client.GetStream();
                    //netStream.Flush();
                    netStream.WriteTimeout = 10;//2500;
                    //lock(synch)
                    netStream.BeginWrite(data, 0, data.Length, WriteAsyncCallback, client);
                    client.WriteDataEvent.Wait(_writeTimeout);
                }
            }
            catch (Exception)
            {
                //todo: umv: add error handling
            }
        }

        private void WriteAsyncCallback(IAsyncResult state)
        {
            TcpClientContext client = state.AsyncState as TcpClientContext;
            if (client == null)
                throw new ApplicationException("state can't be null");
            client.Client.GetStream().EndWrite(state);
            Console.WriteLine("[Server, WriteAsyncCallback] Write done!");
            client.WriteDataEvent.Set();
        }

        private const String DefaultServerIpAddress = "127.0.0.1";
        private const Int32 DefaultServerPort = 16000;
        private const Int32 ServerCloseTimeout = 2000;
        private const Int32 DefaultClientBufferSize = 16384;
        private const Int32 DefaultChunkSize = 1536;
        private const Int32 DefaultClientConnectAttempts = 5;
        private const Int32 DefaultClientConnectTimeout = 200;
        private const Int32 DefaultReadTimrout = 1000;
        private const Int32 DefaultWriteTimeout = 1000;

        // timeouts
        //todo: umv: make adjustable
        private Int32 _clientConnectTimeout = DefaultClientConnectTimeout;
        private Int32 _readTimeout = DefaultReadTimrout;
        private Int32 _writeTimeout = DefaultWriteTimeout;
        // other parameters
        private Int32 _clientConnectAttempts = DefaultClientConnectAttempts;

        private readonly ManualResetEventSlim _clientConnectEvent = new ManualResetEventSlim();
        
        private readonly IList<Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>>>  _clientsHandlers = new List<Tuple<TcpClientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]>>>();
        private readonly IList<TcpClientContext> _tcpClients = new List<TcpClientContext>(); 
        private readonly Object _synch = new Object();
        private String _ipAddress;
        private UInt16 _port;
        private TcpListener _tcpListener;
        private Boolean _interruptRequested;
    }
}
