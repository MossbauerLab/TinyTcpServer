using System;

namespace TinyTcpServer.Api.Client
{
    public class TcpClientInfo
    {
        public TcpClientInfo(Guid id, String ipAddress, Int32 port = -1)
        {
            Id = id;
            IpAddress = ipAddress;
            Port = port;
        }

        public Guid Id { get; private set; }
        public String IpAddress { get; private set; }
        public Int32 Port { get; private set; }
    }
}
