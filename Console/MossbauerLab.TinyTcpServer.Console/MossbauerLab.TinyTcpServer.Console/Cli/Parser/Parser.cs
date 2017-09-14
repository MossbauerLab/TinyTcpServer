using System;
using System.Collections.Generic;
using System.Linq;
using MossbauerLab.TinyTcpServer.Console.Cli.Data;

namespace MossbauerLab.TinyTcpServer.Console.cli.Parser
{
    static class Parser
    {
        public static CommandInfo Parse(String[] args)
        {
            if(args == null)
                throw new NullReferenceException("args");
            IList<String> keys = args.Where(line => line.StartsWith(KeySign)).ToList();
            if (keys.Count == 0)
                throw new ApplicationException("Arguments list doesn't contains any key");
            CommandInfo info = new CommandInfo();
            return info;
        }

        private const String KeySign = "--";
    }
}
