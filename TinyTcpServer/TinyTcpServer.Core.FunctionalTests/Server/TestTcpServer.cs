using System;
using NUnit.Framework;
using TinyTcpServer.Core.Server;

namespace TinyTcpServer.Core.FunctionalTests.Server
{
    [TestFixture]
    public class TestTcpServer
    {
        [Test]
        public void TestStartSuccessfully()
        {
            Boolean result = _server.Start(LocalIpAddress, ServerPort1);
            Assert.IsTrue(result, String.Format("Checking that server was successfully startted on host with ip: {0} , and port: {1}", LocalIpAddress, ServerPort1));
            _server.Stop(true);
            _server.Start(LocalIpAddress, ServerPort2);
            Assert.IsTrue(result, String.Format("Checking that server was successfully startted on host with ip: {0} , and port: {1}", LocalIpAddress, ServerPort2));
            _server.Restart();
        }

        [TestCase (1024, 1)]
        [TestCase(1024, 16)]
        [TestCase(1024, 128)]
        [TestCase(1024, 666)]
        [TestCase(1024, 1024)]
        [TestCase(32768, 20)]
        [TestCase(40000, 10)]
        [TestCase(131072, 8)]
        [TestCase(1048576, 2)]
        public void TestServerExchangeWithOneClient(Int32 dataSize, Int32 repetition)
        {
        }

        private const String LocalIpAddress = "127.0.0.1";
        private const Int32 ServerPort1 = 9999;
        private const Int32 ServerPort2 = 12345;

        private readonly ITcpServer _server = new TcpServer();
    }
}
