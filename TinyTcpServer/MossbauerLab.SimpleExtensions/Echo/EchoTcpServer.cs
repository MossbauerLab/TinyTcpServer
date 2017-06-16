using System;
using log4net;
using MossbauerLab.TinyTcpServer.Core.Handlers;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.SimpleExtensions.Echo
{
    /// <summary>
    ///  RFC 862 Implementation
    /// </summary>
    class EchoTcpServer : TcpServer
    {
        private static class EchoTcpClientHandler
        {
            public static Byte[] Handle(Byte[] data, TcpClientHandlerInfo info)
            {
                lock (data)
                {
                    Byte[] outputData = new Byte[data.Length];
                    Array.Copy(data, outputData, data.Length);
                    return outputData;
                }
            }
        }

        public EchoTcpServer(String ipAddress, UInt16 port = DefaultEchoPort, ILog logger = null, Boolean debug = false)
            : base(ipAddress, port, logger, debug)
        {
            AddHandler(new TcpClientHandlerInfo(Guid.NewGuid()), EchoTcpClientHandler.Handle);
        }

        private const UInt16 DefaultEchoPort = 7;
    }
}
