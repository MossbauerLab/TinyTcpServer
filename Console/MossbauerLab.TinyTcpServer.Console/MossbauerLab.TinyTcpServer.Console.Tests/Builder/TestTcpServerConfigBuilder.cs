using System;
using MossbauerLab.TinyTcpServer.Console.Builders;
using MossbauerLab.TinyTcpServer.Core.Server;
using NUnit.Framework;

namespace MossbauerLab.TinyTcpServer.Console.Tests.Builder
{
    [TestFixture]
    public class TestTcpServerConfigBuilder
    {
        [Test]
        public void TestBuildReadFullConfigs()
        {
            TcpServerConfig expectedConfig = new TcpServerConfig(1000, 200, 1000, 2000, 120, 4, 8, 256, 65535, 4096);
            ReadConfigAndCheck(FullSettingsFile, expectedConfig);
        }

        [Test]
        public void TestBuildReadPartialConfig()
        {
            TcpServerConfig expectedConfig = new TcpServerConfig();
            expectedConfig.ParallelTask = 256;
            expectedConfig.ServerCloseTimeout = 7000;
            ReadConfigAndCheck(PartialSettingsFile, expectedConfig);
        }

        private void ReadConfigAndCheck(String configFile, TcpServerConfig expectedConfig)
        {
            TcpServerConfig actualConfig = TcpServerConfigBuilder.Build(configFile);
            Assert.AreEqual(expectedConfig.ChunkSize, actualConfig.ChunkSize, "ChunkSize");
            Assert.AreEqual(expectedConfig.ClientBufferSize, actualConfig.ClientBufferSize, "ClientBufferSize");
            Assert.AreEqual(expectedConfig.ClientConnectAttempts, actualConfig.ClientConnectAttempts, "ClientConnectAttempts");
            Assert.AreEqual(expectedConfig.ClientConnectTimeout, actualConfig.ClientConnectTimeout, "ClientConnectTimeout");
            Assert.AreEqual(expectedConfig.ClientInactivityTime, actualConfig.ClientInactivityTime, "ClientInactivityTime");
            Assert.AreEqual(expectedConfig.ClientReadAttempts, actualConfig.ClientReadAttempts, "ClientReadAttempts");
            Assert.AreEqual(expectedConfig.ParallelTask, actualConfig.ParallelTask, "ParallelTask");
            Assert.AreEqual(expectedConfig.ReadTimeout, actualConfig.ReadTimeout, "ReadTimeout");
            Assert.AreEqual(expectedConfig.ServerCloseTimeout, actualConfig.ServerCloseTimeout, "ServerCloseTimeout");
            Assert.AreEqual(expectedConfig.WriteTimeout, actualConfig.WriteTimeout, "WriteTimeout");
        }

        private const String FullSettingsFile = @"..\..\TestSettingsFiles\FullSettings.txt";
        private const String PartialSettingsFile = @"..\..\TestSettingsFiles\PartialSettings.txt";
    }
}
