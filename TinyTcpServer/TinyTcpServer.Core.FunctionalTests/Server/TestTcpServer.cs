using System;
using System.Net;
using NUnit.Framework;
using TinyTcpServer.Core.FunctionalTests.TestUtils;
using TinyTcpServer.Core.Handlers;
using TinyTcpServer.Core.Server;

namespace TinyTcpServer.Core.FunctionalTests.Server
{
    [TestFixture]
    public class TestTcpServer
    {
        [SetUp]
        public void SetUp()
        {
            if (_server != null)
                _server.Stop(true);
            _server = null;
            _server = new TcpServer();
        }

        [TearDown]
        public void TearDown()
        {
            if (_server != null)
                _server.Stop(true);
        }

        [Test]
        public void TestStartSuccessfully()
        {
            Boolean result = _server.Start(LocalIpAddress, ServerPort1);
            Assert.IsTrue(result, String.Format("Checking that server was successfully startted on host with ip: {0} , and port: {1}", LocalIpAddress, ServerPort1));
            _server.Stop(true);
            _server.Start(LocalIpAddress, ServerPort2);
            Assert.IsTrue(result, String.Format("Checking that server was successfully startted on host with ip: {0} , and port: {1}", LocalIpAddress, ServerPort2));
            _server.Restart();
            _server.Stop(true);
        }

        [TestCase (1024, 1, true)]
        [TestCase(1024, 2, true)]
        [TestCase(1024, 16, true)]
        [TestCase(1024, 128, true)]
        //[TestCase(1024, 666, true)]
        //[TestCase(1024, 1024, true)] // too long
        [TestCase(8192, 32, true)]
        [TestCase(16384, 32, true)]
        [TestCase(32768, 20, true)]
        [TestCase(40000, 10, true)]
        [TestCase(131072, 8, true)]
        [TestCase(1048576, 2, true)]
        [TestCase(1024, 1, false)]
        [TestCase(1024, 2, false)]
        [TestCase(1024, 16, false)]
        [TestCase(1024, 128, false)]
        //[TestCase(1024, 1024, false)] // too long
        [TestCase(8192, 32, false)]
        [TestCase(16384, 32, false)]
        [TestCase(32768, 20, false)]
        [TestCase(40000, 10, false)]
        [TestCase(131072, 8, false)]
        [TestCase(1048576, 2, false)]
        public void TestServerExchangeWithOneClient(Int32 dataSize, Int32 repetition, Boolean isClientAsync)
        {
            TcpClientHandlerInfo clientHandlerInfo = new TcpClientHandlerInfo(Guid.NewGuid());
            _server.AddHandler(clientHandlerInfo, EchoTcpClientHandler.Handle);
            using (NetworkClient client = new NetworkClient(new IPEndPoint(IPAddress.Parse(LocalIpAddress), ServerPort1),
                                                            isClientAsync, 1000, 
                                                            400, 400))
            {

                Boolean result = _server.Start(LocalIpAddress, ServerPort1);
                Assert.IsTrue(result, "Checking that server was successfully opened");
                client.Open();
                ExchangeWithRandomDataAndCheck(client, dataSize, repetition);
                client.Close();
            }
            _server.Stop(true);
        }

        public void TestServerExchangeWithOneClientWithTimeSpacingBetweenExchange()
        {
        }

        private Byte[] CreateRandomData(Int32 size)
        {
            Byte[] randomData = new Byte[size];
            Random randomGenerator = new Random();
            for (Int32 counter = 0; counter < randomData.Length; counter++)
                randomData[counter] = (Byte)randomGenerator.Next(0, 0x0F);
            return randomData;
        }

        private void ExchangeWithRandomDataAndCheck(NetworkClient client, Int32 dataSize, Int32 repetition)
        {
            for (Int32 repetitionCounter = 0; repetitionCounter < repetition; repetitionCounter++)
            {
                Byte[] expectedData = CreateRandomData(dataSize);
                Byte[] actualData = new Byte[expectedData.Length];
                Int32 bytesReceived;
                Boolean result = client.Write(expectedData);
                Assert.IsTrue(result, String.Format("Checking that client successfully write data, at exchange cycle {0}", repetitionCounter + 1));
                result = client.Read(actualData, out bytesReceived);
                Assert.IsTrue(result, String.Format("Checking that read operation was performed successfully at exchange cycle {0}", repetitionCounter + 1));
                Assert.AreEqual(expectedData.Length, bytesReceived, String.Format("Chechking that client received expected number of bytes at exchange cycle {0}", repetitionCounter + 1));
                for (Int32 counter = 0; counter < expectedData.Length; counter++)
                    Assert.AreEqual(expectedData[counter], actualData[counter], String.Format("Checking that arrays bytes are equals at index {0}, at exchange cycle {1}", counter,  repetitionCounter + 1));
            }
        }

        private const String LocalIpAddress = "127.0.0.1";
            //"127.0.0.1";
        private const Int32 ServerPort1 = 9999;
        private const Int32 ServerPort2 = 12345;
        //private const String ClientIpAddress = "127.0.0.1";

        private ITcpServer _server;
    }
}
