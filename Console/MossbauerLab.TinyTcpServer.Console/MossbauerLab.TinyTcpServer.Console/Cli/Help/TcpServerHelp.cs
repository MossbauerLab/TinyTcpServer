using System;

namespace MossbauerLab.TinyTcpServer.Console.Cli.Help
{
    static class TcpServerHelp
    {
        public static String HelpMessage = @"This is Tcp server console, we are using flexible Tcp Server (with logic inside C# script (class)) \r\n
     There are following options:
         1) Start: --start --ipaddr=192.168.10.34 --port=7659 --script=EchoScript.cs --settings=Setting.txt
            for first time ipaddr, port and script MUST be set, but if server was initially run and then \r\n
            stopped, it could be run simply --start (with same setting).
            every script MUST contain Entry Point -> class ServerScript and method public void Init(ref ITcpServer server) \r\n
            For more information about settings see github wiki (https://github.com/MossbauerLab/TinyTcpServer) \r\n
         2) Stop: --stop only started server could be stopped \r\n
         3) Restart: --restart restart is equivalent --stop plus --start, only started server could be restart.";
    }
}
