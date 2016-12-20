using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TinyTcpServer.Api.Client;

namespace TinyTcpServer.Api.Server
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
            foreach (TcpClient tcpClient in _tcpClients)
            {
                tcpClient.GetStream().Flush();
                tcpClient.GetStream().Close();
                tcpClient.Client.Close();
            }
            _tcpClients.Clear();
        }

        private void ReleaseClientsHandlers()
        {
            _clientsHandlers.Clear();
        }

        private void StartClientProcessing()
        {
            // 1. waiting for connection ...
            // 2. handle clients ...
            // 3. check "disconnected" clients ...
        }

        private const String DefaultServerIpAddress = "127.0.0.1";
        private const Int32 DefaultServerPort = 16000;
        private const Int32 ServerCloseTimeout = 2000;
        
        private readonly IList<Tuple<TcpClientInfo, Func<Byte[], TcpClientInfo, Byte[]>>>  _clientsHandlers = new List<Tuple<TcpClientInfo, Func<Byte[], TcpClientInfo, Byte[]>>>();
        private readonly IList<TcpClient> _tcpClients = new List<TcpClient>(); 
        private String _ipAddress;
        private UInt16 _port;
        private TcpListener _tcpListener;

    }
}
