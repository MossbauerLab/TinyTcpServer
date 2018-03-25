using System;
using System.Collections.Generic;
using Microsoft.CSharp;
using MossbauerLab.TinyTcpServer.Core.FunctionalTests.TestUtils;
using MossbauerLab.TinyTcpServer.Core.Scripting;
using MossbauerLab.TinyTcpServer.Core.Server;
using NUnit.Framework;

namespace MossbauerLab.TinyTcpServer.Core.FunctionalTests.Server
{
    [TestFixture]
    public class TestFlexibleTcpServer
    {
        [TestCase(16, 1, true)]
        [TestCase(32, 32, true)]
        [TestCase(1024, 1, true)]
        [TestCase(1024, 2, true)]
        [TestCase(1024, 16, true)]
        [TestCase(8192, 32, true)]
        [TestCase(16384, 32, true)]
        [TestCase(32768, 20, true)]
        [TestCase(40000, 10, true)]
        [TestCase(131072, 8, true)]
        [TestCase(1048576, 2, true)]
        [TestCase(16, 1, false)]
        [TestCase(32, 32, false)]
        [TestCase(1024, 1, false)]
        [TestCase(1024, 2, false)]
        [TestCase(1024, 16, false)]
        [TestCase(8192, 32, false)]
        [TestCase(16384, 32, false)]
        [TestCase(32768, 20, false)]
        [TestCase(40000, 10, false)]
        [TestCase(131072, 8, false)]
        [TestCase(1048576, 2, false)]
        public void TestScriptRun(Int32 dataSize, Int32 repetition, Boolean isClientAsync)
        {
            _server = new FlexibleTcpServer(DefaultScript, LocalIpAddress, ServerPort);
            Boolean result = _server.Start();
            Assert.IsTrue(result, "Checking that result is true");
            using (TransportClient client = new TransportClient(isClientAsync, LocalIpAddress, ServerPort))
            {
                client.Open();
                ExchangeWithRandomDataAndCheck(client, dataSize, repetition);
                client.Close();
            }
            Assert.DoesNotThrow(() => _server.Stop(true), "Checking that server correctly stops");
        }

        [TestCase(16, 1, true)]
        [TestCase(32, 32, true)]
        [TestCase(1024, 1, true)]
        [TestCase(1024, 2, true)]
        [TestCase(1024, 16, true)]
        [TestCase(16, 1, false)]
        [TestCase(32, 32, false)]
        [TestCase(1024, 1, false)]
        [TestCase(1024, 2, false)]
        [TestCase(1024, 16, false)]
        [TestCase(8192, 32, false)]
        public void TestScriptRunCustomCompilerSettings(Int32 dataSize, Int32 repetition, Boolean isClientAsync)
        {
            CompilerOptions compilerOptions = new CompilerOptions();
            compilerOptions.Provider = new CSharpCodeProvider(new Dictionary<String, String>()
            {
                {"CompilerVersion", "v4.0"}
            });
            compilerOptions.Parameters.GenerateExecutable = false;
            compilerOptions.Parameters.GenerateInMemory = true;
            compilerOptions.ScriptEntryType = "MossbauerLab.TinyTcpServer.Core.FunctionalTests.TestScripts.CustomScript";

            _server = new FlexibleTcpServer(CustomScript, LocalIpAddress, ServerPort, compilerOptions);
            Boolean result = _server.Start();
            Assert.IsTrue(result, "Checking that result is true");
            using (TransportClient client = new TransportClient(isClientAsync, LocalIpAddress, ServerPort))
            {
                client.Open();
                ExchangeWithRandomDataAndCheck(client, dataSize, repetition);
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

        private const String LocalIpAddress = "127.0.0.1";
        private const UInt16 ServerPort = 8044;
        private const String DefaultScript = @"..\..\TestScripts\SimpleScript.cs";
        private const String CustomScript = @"..\..\TestScripts\CustomScript.cs";

        private ITcpServer _server;
    }
}
