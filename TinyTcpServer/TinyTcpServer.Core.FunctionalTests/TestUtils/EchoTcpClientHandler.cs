using System;
using TinyTcpServer.Core.Handlers;

namespace TinyTcpServer.Core.FunctionalTests.TestUtils
{
    internal static class EchoTcpClientHandler
    {
        public static Byte[] Handle(Byte[] data, TcpClientHandlerInfo info)
        {
            return data;
        }
    }
}
