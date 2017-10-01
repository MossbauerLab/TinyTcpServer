using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using MossbauerLab.TinyTcpServer.Console.Cli.Data;
using MossbauerLab.TinyTcpServer.Console.Cli.Options;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace MossbauerLab.TinyTcpServer.Console.Tests.Validator
{
    [TestFixture]
    public class TestValidator
    {
        [TestCase(CommandType.Start, "192.168.66.66", 1134U, "HttpScript.cs", null, false)]
        [TestCase(CommandType.Start, null, null, "HttpScript.cs", null, false)]
        [TestCase(CommandType.Start, null, null, "HttpScript.cs", "Settings.txt", false)]
        [TestCase(CommandType.Start, null, null, null, null, true)]
        [TestCase(CommandType.Stop, null, null, null, null, true)]
        [TestCase(CommandType.Restart, null, null, null, null, true)]
        [TestCase(CommandType.Restart, "192.168.66.66", 1134U, null, null, true)]
        [TestCase(CommandType.Quit, null, null, null, null, true)]
        [TestCase(CommandType.Help, null, null, null, null, true)]
        [TestCase(CommandType.Quit, null, null, null, null, false)]
        [TestCase(CommandType.Help, null, null, null, null, false)]
        public void TestSuccessfulValidate(CommandType command, String ipAddress, Object port, String script, String settings, Boolean serverInited)
        {
            TestValidateImpl(command, ipAddress, port, script, settings, serverInited, true);
        }

        [TestCase(CommandType.Start, null, null, null, null, false)]
        [TestCase(CommandType.Stop, "192.168.66.66", 1134U, null, null, false)]
        [TestCase(CommandType.Restart, "192.168.66.66", 1134U, "FtpServer.cs", null, false)]
        [TestCase(CommandType.Restart, null, null, "FtpServer.cs", null, true)]
        [TestCase(CommandType.Restart, null, null, null, null, false)]
        [TestCase(CommandType.Quit, "192.168.66.66", null, null, null, true)]
        [TestCase(CommandType.Help, "192.168.66.66", null, null, null, true)]
        [TestCase(CommandType.Quit, "192.168.66.66", null, null, null, false)]
        [TestCase(CommandType.Help, "192.168.66.66", null, null, null, false)]
        public void TestValidateFails(CommandType command, String ipAddress, Object port, String script, String settings, Boolean serverInited)
        {
            TestValidateImpl(command, ipAddress, port, script, settings, serverInited, false);
        }

        private void TestValidateImpl(CommandType command, String ipAddress, Object port, String script, String settings,
                                      Boolean serverInited, Boolean expectedResult)
        {
            UInt32? portValue = null;
            if (port != null)
                portValue = (UInt32?)port;
            CommandInfo info = GetCommandInfo(command, ipAddress, (UInt16?)portValue, script, settings);
            Boolean result = Cli.Validator.Validator.Validate(info, serverInited);
            if (expectedResult)
                Assert.IsTrue(result, "Checking that validation was successful");
            else Assert.IsFalse(result, "Checking that validation failed");
        }

        private CommandInfo GetCommandInfo(CommandType command, String ipAddress, UInt16? port, String script, String settings)
        {
            CommandInfo info = new CommandInfo();
            info.Command = command;
            info.IpAddress = ipAddress;
            info.Port = port;
            info.ScriptFile = script;
            info.SettingsFile = settings;
            return info;
        }
    }
}
