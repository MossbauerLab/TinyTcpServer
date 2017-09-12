using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MossbauerLab.TinyTcpServer.Core.Server;
using NUnit.Framework;

namespace MossbauerLab.TinyTcpServer.Core.FunctionalTests.Server
{
    [TestFixture]
    public class TestFlexibleTcpServer
    {
        [Test]
        public void TestScripyRun()
        {
            const String script = @"..\..\TestScripts\SimpleScript.cs";
            _server = new FlexibleTcpServer(script);
            Boolean result = _server.Start();
            Assert.IsTrue(result, "Checking that result is true");
            Assert.DoesNotThrow(() => _server.Stop(true), "Checking that server correctly stops");
        }

        private ITcpServer _server;
    }
}
