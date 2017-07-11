using System;
using System.Text;
using log4net;
using log4net.Config;
using MossbauerLab.SimpleExtensions.Echo;
using MossbauerLab.TinyTcpServer.Core.FunctionalTests.Server;
using MossbauerLab.TinyTcpServer.Core.FunctionalTests.TestUtils;
using MossbauerLab.TinyTcpServer.Core.Server;
using NUnit.Framework;

namespace MossbauerLab.SimpleExtensions.Tests.Echo
{
    [TestFixture]
    class TestEchoTcpServer
    {
        /*public TestEchoTcpServer()
        {
            XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(TestTcpServer));
            _echoServer = new EchoTcpServer(LocalHost, EchoServerPort, _logger, true);
        }*/

        [SetUp]
        public void SetUp()
        {
            XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(TestTcpServer));
            _echoServer = new EchoTcpServer(LocalHost, EchoServerPort, _logger, true);
            _echoServer.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _echoServer.Stop(true);
        }

        [TestCase("Ololo ololo i am driver of UFO!")]
        [TestCase("Some simple message with digits 01234567890ABCDEF (hex alphabet)")]
        [TestCase("HOP HEY LALALEI")]
        public void TestEcho(String message)
        {
            using (TransportClient client = new TransportClient(true, LocalHost, EchoServerPort, 500, 500))
            {
                while (!_echoServer.IsReady) ;
                Boolean result = client.Open();
                Assert.IsTrue(result, "Checking that connection established");
                result = client.Write(Encoding.UTF8.GetBytes(message));
                Assert.IsTrue(result, "Checking that write was performed successfully");
                Byte[] backMessage = new Byte[1024];
                Int32 bytesReceived;
                result = client.Read(backMessage, out bytesReceived);
                Assert.IsTrue(result, "Checking that read was performed successfully");
                Assert.AreEqual(message.Length, bytesReceived, "Checking that we have received equal number of bytes");
                client.Close();
            }
        }


        private const UInt16 EchoServerPort = 10000;
        private const String LocalHost = "127.0.0.1";
        private ILog _logger;
        private ITcpServer _echoServer;
    }
}
