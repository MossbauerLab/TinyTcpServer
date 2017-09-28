using System;
using MossbauerLab.TinyTcpServer.Console.Builders;
using MossbauerLab.TinyTcpServer.Console.cli.Parser;
using MossbauerLab.TinyTcpServer.Console.Cli.Data;
using MossbauerLab.TinyTcpServer.Console.Cli.Options;
using MossbauerLab.TinyTcpServer.Console.Cli.Validator;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.TinyTcpServer.Console
{
    [Flags]
    public enum State
    {
        Initial,
        Initialized,
        Started,
        Stopped
    }

    public class Program
    {
        public static void Main(String[] args)
        {
            const UInt16 defaultPort = 6666;
            State serverState = State.Initial;
            Boolean terminate = false;
            ITcpServer server = null;
            TcpServerConfig lastConfig;
            try
            {
                while (!terminate)
                {
                    CommandInfo info = Parser.Parse(args);
                    Boolean result = Validator.Validate(info, serverState >= State.Initialized);
                    if (!result)
                        System.Console.WriteLine("Incorrect syntax, see --help for details");
                    else
                    {
                        if (info.Command == CommandType.Quit)
                        {
                            server = null;
                            terminate = true;
                        }

                        else if (info.Command == CommandType.Start && serverState != State.Started)
                        {
                            lastConfig = info.ScriptFile != null ? TcpServerConfigBuilder.Build(info.ScriptFile) : null;
                            if(server == null || info.ScriptFile != null)
                                server = new FlexibleTcpServer(info.ScriptFile, info.IpAddress, info.Port ?? defaultPort, null, false, lastConfig);
                            serverState = State.Started;
                            server.Start();
                        }

                        else if (info.Command == CommandType.Stop && serverState == State.Started)
                        {
                            if(server!=null)
                                server.Stop(true);
                            serverState = State.Stopped;
                        }

                        else if (info.Command == CommandType.Restart && serverState == State.Started)
                        {
                            if (server != null)
                            {
                                server.Stop(true);
                                if (info.IpAddress != null && info.Port != null)
                                    server.Start(info.IpAddress, info.Port.Value);
                                else server.Start();
                            }
                        }

                        else if (info.Command == CommandType.Help)
                        {
                            // todo: umv: add help display
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("An error occured during server work: " + e.Message);
                throw;
            }
           
        }
    }
}
