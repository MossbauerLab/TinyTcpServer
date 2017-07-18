using System;
using log4net;
using MossbauerLab.SimpleExtensions.Echo;
using MossbauerLab.SimpleExtensions.Time;
using MossbauerLab.TinyTcpServer.Core.Server;
using MossbauerLab.TinyTcpServer.MnGUI.Data;

namespace MossbauerLab.TinyTcpServer.MnGUI.Factories
{
    public static class ServerFactory
    {
        public static ITcpServer Create(ServerType type, String ipAddress, UInt16 port, ILog logger = null)
        {
            if(type == ServerType.Echo)
                return new EchoTcpServer(ipAddress, port, logger);
            if(type == ServerType.Time)
                return new TimeTcpServer(ipAddress, port, logger);
            throw new NotImplementedException("Other types were not implemented");
        }
    }
}
