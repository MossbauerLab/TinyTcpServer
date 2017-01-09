using System;

namespace MossbauerLab.TinyTcpServer.Core.Handlers
{
    public class TcpClientHandlerInfo
    {
        public TcpClientHandlerInfo(Guid id, String ipAddress = GlobalDefs.AnyIpAddress, Int32 port = GlobalDefs.AnyPort)
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
