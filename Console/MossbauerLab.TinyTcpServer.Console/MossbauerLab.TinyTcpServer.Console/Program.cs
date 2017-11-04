using System;
using System.Text;
using log4net;
using log4net.Config;
using MossbauerLab.TinyTcpServer.Console.StateMachine;

namespace MossbauerLab.TinyTcpServer.Console
{
    public class Program
    {
        public static void Main(String[] args)
        {
            InitLogger();
            TcpServerStateMachine machine = new TcpServerStateMachine(null, _logger);
            machine.DefaultInit();

            try
            {
                StringBuilder inputStr = new StringBuilder();
                foreach (String arg in args)
                    inputStr.Append(arg + ' ');
                while (true)
                {
                    Boolean result = machine.Run(inputStr);
                    if(!result)
                        break;
                    System.Console.WriteLine("Waiting for next command, or --quit for exit, see --help");
                    String input = System.Console.ReadLine();
                    if (input == null)
                        throw new ApplicationException("User input is null");
                    inputStr.Clear();
                    inputStr.Append(input);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("An error occured during server work: " + e.Message);
                throw;
            }
        }

        private static void InitLogger()
        {
            XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(Program));
        }

        private static ILog _logger;
    }
}
