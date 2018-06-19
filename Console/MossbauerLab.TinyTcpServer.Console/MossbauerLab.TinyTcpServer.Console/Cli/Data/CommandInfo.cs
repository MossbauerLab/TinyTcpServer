using System;
using MossbauerLab.TinyTcpServer.Console.Cli.Options;

namespace MossbauerLab.TinyTcpServer.Console.Cli.Data
{
    public class CommandInfo
    {
        public CommandInfo()
        {
            IpAddress = null;
            Port = null;
            ScriptFile = null;
            SettingsFile = null;
        }

        public CommandType Command { get; set; }

        public String IpAddress { get; set; }
        public UInt16? Port { get; set; }
        public String ScriptFile { get; set; }
        public String SettingsFile { get; set; }  // server setting file
        public String CompilerOptionsFile { get; set; }
    }
}
