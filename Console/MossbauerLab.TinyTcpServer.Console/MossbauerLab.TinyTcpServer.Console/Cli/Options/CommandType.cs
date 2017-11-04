using System;

namespace MossbauerLab.TinyTcpServer.Console.Cli.Options
{
    [Flags]
    public enum CommandType : byte
    {
        Start,
        Stop,
        Restart,
        Help,
        Quit
    }
}
