using System;

namespace TinyTcpServer.Api.Client
{
    public class TcpClientInfo
    {
        public TcpClientInfo(String ipAddress, Int32 port = -1)
        {
            IpAddress = ipAddress;
            Port = port;
        }

        public String IpAddress { get; private set; }
        public Int32 Port { get; private set; }
    }
}
