using System;
using MossbauerLab.TinyTcpServer.Console.Cli.Options;

namespace MossbauerLab.TinyTcpServer.Console.Cli.Data
{
    public class CommandInfo
    {
        public CommandType Command { get; set; }

        public String IpAddress { get; set; }
        public UInt16 Port { get; set; }
        public String ScriptFile { get; set; }
        public String SettingsFile { get; set; }  // server setting file
    }
}
