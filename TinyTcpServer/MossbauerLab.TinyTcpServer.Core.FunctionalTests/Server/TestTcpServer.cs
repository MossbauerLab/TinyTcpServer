using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using MossbauerLab.TinyTcpServer.Core.FunctionalTests.TestUtils;
using MossbauerLab.TinyTcpServer.Core.Handlers;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.TinyTcpServer.Core.FunctionalTests.Server
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
            _server.AddHandler(_clientHandlerInfo, EchoTcpClientHandler.Handle);
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
        //[TestCase(1024, 666, true)] // too long
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
            using (NetworkClient client = new NetworkClient(new IPEndPoint(IPAddress.Parse(LocalIpAddress), ServerPort1), isClientAsync, 1000, 500, 250))
            {

                Boolean result = _server.Start(LocalIpAddress, ServerPort1);
                Assert.IsTrue(result, "Checking that server was successfully opened");
                client.Open();
                ExchangeWithRandomDataAndCheck(client, dataSize, repetition);
                client.Close();
            }
            _server.Stop(true);
        }

        [TestCase(1024, 16, 30, 50, true)]
        [TestCase(1024, 128, 10, 40, true)]
        [TestCase(8192, 32, 70, 100, true)]
        [TestCase(16384, 32, 40, 100, true)]
        [TestCase(32768, 20, 40, 100, true)]
        [TestCase(40000, 10, 100, 150, true)]
        [TestCase(1024, 16, 30, 50, false)]
        [TestCase(1024, 128, 10, 40, false)]
        [TestCase(8192, 32, 70, 100, false)]
        [TestCase(16384, 32, 40, 100, false)]
        [TestCase(32768, 20, 40, 100, false)]
        [TestCase(40000, 10, 100, 150, false)]
        public void TestServerExchangeWithPausesAndOneClient(Int32 dataSize, Int32 repetition, Int32 minPauseTime, Int32 maxPauseTime, Boolean isClientAsync)
        {
            using (NetworkClient client = new NetworkClient(new IPEndPoint(IPAddress.Parse(LocalIpAddress), ServerPort1), isClientAsync, 1000, 500, 250))
            {

                Boolean result = _server.Start(LocalIpAddress, ServerPort1);
                Assert.IsTrue(result, "Checking that server was successfully opened");
                client.Open();
                ExchangeWithRandomDataAndCheck(client, dataSize, repetition, minPauseTime, maxPauseTime);
                client.Close();
            }
            _server.Stop(true);
        }

        [TestCase(2, 1024, 16, true)]
        [TestCase(64, 1024, 1, true)]
        [TestCase(32, 1024, 16, true)]
        [TestCase(16, 8192, 32, true)]
        [TestCase(16, 16384, 32, true)]
        [TestCase(8, 131072, 8, true)]
        [TestCase(4, 1048576, 2, true)]
        [TestCase(2, 1024, 16, false)]
        [TestCase(64, 1024, 1, false)]
        [TestCase(32, 1024, 16, false)]
        [TestCase(16, 8192, 32, false)]
        [TestCase(16, 16384, 32, false)]
        [TestCase(8, 131072, 8, false)]
        [TestCase(4, 1048576, 2, false)]
        public void TestServerExchangeWithSeveralClients(Int32 numberOfClients, Int32 dataSize, Int32 repetition, Boolean isClientAsync)
        {
            Boolean result = _server.Start(LocalIpAddress, ServerPort1);
            Assert.IsTrue(result, "Checking that server was successfully opened");
            Task[] clientTasks = new Task[numberOfClients];
            for (Int32 clientCounter = 0; clientCounter < numberOfClients; clientCounter++)
            {
                Task clientTask = new Task(() =>
                {
                    using (NetworkClient client = new NetworkClient(new IPEndPoint(IPAddress.Parse(LocalIpAddress), ServerPort1), isClientAsync, 2000, 1500, 1500))
                    {
                        client.Open();
                        ManualResetEventSlim openWaitEvent = new ManualResetEventSlim();
                        while (_server.ConnectedClients != numberOfClients)
                        {
                            openWaitEvent.Reset();
                            openWaitEvent.Wait(10);
                        }
                        openWaitEvent.Dispose();
                        // wait 4 getting a chance for client to be ready for IO with server
                        for (Int32 counter = 0; counter < repetition; counter++)
                        {
                            SingleExchangeWithRandomDataAndCheck(client, dataSize, counter);
                        }
                        client.Close();
                    }
                });
                clientTasks[clientCounter] = clientTask;
                clientTask.Start();
            }
            Task.WaitAll(clientTasks, -1);
            foreach (Task clientTask in clientTasks)
                clientTask.Dispose();
            _server.Stop(true);
        }

        private Byte[] CreateRandomData(Int32 size)
        {
            Byte[] randomData = new Byte[size];
            Random randomGenerator = new Random();
            for (Int32 counter = 0; counter < randomData.Length; counter++)
                randomData[counter] = (Byte)randomGenerator.Next(0, 0x0F);
            return randomData;
        }

        private void ExchangeWithRandomDataAndCheck(NetworkClient client, Int32 dataSize, Int32 repetition, Int32 pauseMin = 0, Int32 pauseMax = 0)
        {
            Random pauseRandomGenerator = new Random();
            for (Int32 repetitionCounter = 0; repetitionCounter < repetition; repetitionCounter++)
            {
                SingleExchangeWithRandomDataAndCheck(client, dataSize, repetitionCounter);
                if (pauseMin > 0 && pauseMax > 0)
                {
                    Int32 pause = pauseRandomGenerator.Next(pauseMin, pauseMax);
                    TimeDelay.Delay(pause);
                }
            }
        }

        private void SingleExchangeWithRandomDataAndCheck(NetworkClient client, Int32 dataSize, Int32 cycle)
        {
            Byte[] expectedData = CreateRandomData(dataSize);
            Byte[] actualData = new Byte[expectedData.Length];
            Int32 bytesReceived;
            Boolean result = client.Write(expectedData);
            Assert.IsTrue(result, String.Format("Checking that client successfully write data, at exchange cycle {0}", cycle + 1));
            result = client.Read(actualData, out bytesReceived);
            Assert.IsTrue(result, String.Format("Checking that read operation was performed successfully at exchange cycle {0}", cycle + 1));
            Assert.AreEqual(expectedData.Length, bytesReceived, String.Format("Chechking that client received expected number of bytes at exchange cycle {0}", cycle + 1));
            for (Int32 counter = 0; counter < expectedData.Length; counter++)
                Assert.AreEqual(expectedData[counter], actualData[counter], String.Format("Checking that arrays bytes are equals at index {0}, at exchange cycle {1}", counter, cycle + 1));
        }

        private const String LocalIpAddress = "127.0.0.1";
        private const Int32 ServerPort1 = 9999;
        private const Int32 ServerPort2 = 12345;

        private ITcpServer _server;
        private readonly TcpClientHandlerInfo _clientHandlerInfo = new TcpClientHandlerInfo(Guid.NewGuid());
    }
}
