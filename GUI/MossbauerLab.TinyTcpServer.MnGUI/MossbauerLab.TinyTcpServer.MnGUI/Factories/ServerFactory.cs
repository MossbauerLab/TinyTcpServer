using System;
using log4net;
using MossbauerLab.TinyTcpServer.Core.Scripting;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.TinyTcpServer.MnGUI.Factories
{
    public static class ServerFactory
    {
        public static ITcpServer Create(String ipAddress, UInt16 port, String serverScriptFile, ILog logger = null, 
                                        CompilerOptions compilerOptions = null, TcpServerConfig config = null)
        {
            return new FlexibleTcpServer(serverScriptFile, ipAddress, port, compilerOptions, logger, false, config);
        }
    }
}
