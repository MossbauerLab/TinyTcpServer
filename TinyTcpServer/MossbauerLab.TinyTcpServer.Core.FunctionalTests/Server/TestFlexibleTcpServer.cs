using System;
using MossbauerLab.TinyTcpServer.Core.FunctionalTests.TestUtils;
using MossbauerLab.TinyTcpServer.Core.Server;
using NUnit.Framework;

namespace MossbauerLab.TinyTcpServer.Core.FunctionalTests.Server
{
    [TestFixture]
    public class TestFlexibleTcpServer
    {
        [Test]
        public void TestScriptRun()
        {
            const String script = @"..\..\TestScripts\SimpleScript.cs";
            _server = new FlexibleTcpServer(script, "127.0.0.1", 8044);
            Boolean result = _server.Start();
            Assert.IsTrue(result, "Checking that result is true");
            using (TransportClient client = new TransportClient(true, "127.0.0.1", 8044))
            {
                client.Open();
                ExchangeWithRandomDataAndCheck(client, 2055, 1);
                client.Close();
            }
            Assert.DoesNotThrow(() => _server.Stop(true), "Checking that server correctly stops");
        }

        private Byte[] CreateRandomData(Int32 size)
        {
            Byte[] randomData = new Byte[size];
            Random randomGenerator = new Random();
            for (Int32 counter = 0; counter < randomData.Length; counter++)
                randomData[counter] = (Byte)randomGenerator.Next(0, 0x0F);
            return randomData;
        }

        private void ExchangeWithRandomDataAndCheck(TransportClient client, Int32 dataSize, Int32 repetition, Int32 pauseMin = 0, Int32 pauseMax = 0)
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

        private void SingleExchangeWithRandomDataAndCheck(TransportClient client, Int32 dataSize, Int32 cycle)
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

        private ITcpServer _server;
    }
}
