using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TinyTcpServer.Core.Client;

namespace TinyTcpServer.Core.Server
{
    public class TinyTcpServer : ITcpServer
    {
        public TinyTcpServer(String ipAddress = DefaultServerIpAddress, UInt16 port = DefaultServerPort)
        {
            AssignIpAddressAndPort(ipAddress, port);
        }

        public Boolean Start(String ipAddress, UInt16 port)
        {
            try
            {
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

        public void AddHandler(TcpClientInfo clientInfo, Func<Byte[], TcpClientInfo, Byte[]> handler)
        {
            if(clientInfo == null)
                throw new ArgumentNullException("clientInfo");
            if(handler == null)
                throw new ArgumentNullException("handler");
            Tuple<TcpClientInfo, Func<Byte[], TcpClientInfo, Byte[]>> existingHandler =_clientsHandlers.FirstOrDefault(item => item.Item1.Id.Equals(clientInfo.Id));
            if (existingHandler == null)
                _clientsHandlers.Add(new Tuple<TcpClientInfo, Func<Byte[], TcpClientInfo, Byte[]>>(clientInfo, handler));
        }

        public void RemoveHandler(TcpClientInfo clientInfo)
        {
            Tuple<TcpClientInfo, Func<Byte[], TcpClientInfo, Byte[]>> handler =_clientsHandlers.FirstOrDefault(item => item.Item1.Id.Equals(clientInfo.Id));
            if (handler != null)
                _clientsHandlers.Remove(handler);
        }

        public void SendData(TcpClientInfo clientInfo, Byte[] data)
        {
            throw new NotImplementedException();
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
            while (true)
            {
                // 1. waiting for connection ...
                Task.Factory.StartNew(ClientConnectProcessing);
                // 2. handle clients ... (read + write)
                for (Int32 clientCounter = 0; clientCounter < _tcpClients.Count; clientCounter++)
                {
                    if (CheckClientConnected(_tcpClients[clientCounter].Client))
                    {
                        TcpClientContext client = _tcpClients[clientCounter];
                        Task.Factory.StartNew(() => ProcessClientReceiveSend(client));
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
                if(client.Connected)
                    _tcpClients.Add(new TcpClientContext(client));
            }
            catch (Exception)
            {
                //todo: umv: probably we should notify i.e. via logs
            }
            _clientConnectEvent.Set();
        }

        private Boolean CheckClientConnected(TcpClient client)
        {
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
                IList<Tuple<TcpClientInfo, Func<Byte[], TcpClientInfo, Byte[]>>>  linkedHandlers =
                _clientsHandlers.Where(item =>
                {
                    //todo: umv: add special selection for AnyPort and AnyIp
                    Boolean ipCheck= String.Equals(item.Item1.IpAddress, ((IPEndPoint) client.Client.Client.RemoteEndPoint).Address.ToString());
                    Boolean portCheck = item.Item1.Port == ((IPEndPoint) client.Client.Client.RemoteEndPoint).Port;
                    Boolean result = ipCheck && portCheck;
                    return result;
                }).ToList();

                foreach (Tuple<TcpClientInfo, Func<Byte[], TcpClientInfo, Byte[]>> handler in linkedHandlers)
                {
                    Byte[] dataForSend = handler.Item2(receivedData, handler.Item1);
                    //todo: unv: add send
                }
            }
        }

        private Byte[] ReceiveImpl(TcpClientContext client)
        {
            Byte[] buffer = new Byte[DefaultClientBufferSize];
            client.ReadDataEvent.Reset();
            NetworkStream netStream = client.Client.GetStream();
            Object synch = new Object();
            client.BytesRead = 0;

            try
            {
                while (netStream.DataAvailable || client.Client.Client.Poll(10000, SelectMode.SelectRead))
                {
                    //Console.WriteLine("thread id: " + Thread.CurrentThread.ManagedThreadId);
                    if (buffer.Length < client.BytesRead + DefaultChunkSize)
                        Array.Resize(ref buffer, buffer.Length + 10*DefaultChunkSize);
                    Int32 offset = client.BytesRead;
                    Int32 size = DefaultChunkSize;
                    //Console.WriteLine("read op, offset " + offset + " size " + size);
                    lock (synch)
                        netStream.BeginRead(buffer, offset, size, ReadAsyncCallback, client);
                    client.ReadDataEvent.Wait(_readTimeout);
                }
                Array.Resize(ref buffer, client.BytesRead);
            }
            catch (Exception)
            {
                // todo: umv: add exception handling ....
                buffer = null;
            }

            return buffer;
        }

        private void ReadAsyncCallback(IAsyncResult state)
        {
            TcpClientContext client = state as TcpClientContext;
            if(client == null)
                throw new ApplicationException("state can't be null");
            client.BytesRead +=client.Client.GetStream().EndRead(state);
            client.ReadDataEvent.Set();
            //Console.WriteLine("bytes read in read callback: " + _bytesRead);
            //Console.WriteLine("thread id: " + Thread.CurrentThread.ManagedThreadId);
        }

        private const String DefaultServerIpAddress = "127.0.0.1";
        private const Int32 DefaultServerPort = 16000;
        private const Int32 ServerCloseTimeout = 2000;
        private const Int32 DefaultClientBufferSize = 16384;
        private const Int32 DefaultChunkSize = 1536;
        private const Int32 DefaultClientConnectAttempts = 5;
        private const Int32 DefaultClientConnectTimeout = 200;
        private const Int32 DefaultReadTimrout = 1000;

        // timeouts
        //todo: umv: make adjustable
        private Int32 _clientConnectTimeout = DefaultClientConnectTimeout;
        private Int32 _readTimeout = DefaultReadTimrout;
        // other parameters
        private Int32 _clientConnectAttempts = DefaultClientConnectAttempts;

        private readonly ManualResetEventSlim _clientConnectEvent = new ManualResetEventSlim();
        
        private readonly IList<Tuple<TcpClientInfo, Func<Byte[], TcpClientInfo, Byte[]>>>  _clientsHandlers = new List<Tuple<TcpClientInfo, Func<Byte[], TcpClientInfo, Byte[]>>>();
        private readonly IList<TcpClientContext> _tcpClients = new List<TcpClientContext>(); 
        private readonly Object _synch = new Object();
        private String _ipAddress;
        private UInt16 _port;
        private TcpListener _tcpListener;
    }
}
