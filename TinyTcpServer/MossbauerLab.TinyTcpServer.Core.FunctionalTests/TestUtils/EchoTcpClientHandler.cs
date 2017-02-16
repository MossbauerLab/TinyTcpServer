using System;
using MossbauerLab.TinyTcpServer.Core.Handlers;

namespace MossbauerLab.TinyTcpServer.Core.FunctionalTests.TestUtils
{
    internal static class EchoTcpClientHandler
    {
        public static Byte[] Handle(Byte[] data, TcpClientHandlerInfo info)
        {
            Byte[] outputData = new Byte[data.Length];
            lock(data)
                Array.Copy(data, outputData, data.Length);
            return outputData;
        }
    }
}
