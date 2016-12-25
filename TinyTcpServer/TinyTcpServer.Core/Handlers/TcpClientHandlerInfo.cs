using System;

namespace TinyTcpServer.Core.Handlers
{
    public class TcpClientHandlerInfo
    {
        public TcpClientHandlerInfo(Guid id, String ipAddress, Int32 port = -1)
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
