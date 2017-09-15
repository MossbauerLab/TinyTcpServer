using System;
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
            State serverState = State.Initial;
            Boolean terminate = false;
            ITcpServer server;
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
                            terminate = true;
                        }

                        else if (info.Command == CommandType.Start)
                        {

                        }

                        else if (info.Command == CommandType.Stop)
                        {

                        }

                        else if (info.Command == CommandType.Restart)
                        {

                        }

                        else if (info.Command == CommandType.Help)
                        {

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
