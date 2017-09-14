using System;
using System.Collections.Generic;
using System.Linq;
using MossbauerLab.TinyTcpServer.Console.Cli.Data;
using MossbauerLab.TinyTcpServer.Console.Cli.Options;

namespace MossbauerLab.TinyTcpServer.Console.cli.Parser
{
    static class Parser
    {
        public static CommandInfo Parse(String[] args)
        {
            if(args == null)
                throw new NullReferenceException("args");
            IList<String> keys = args.Where(line => line.StartsWith(KeySign)).ToList();
            IList<String> keyValuePairs = args.Where(line => !line.StartsWith(KeySign)).ToList();
            if (keys.Count == 0)
                throw new ApplicationException("Arguments list doesn't contains any key");
            if(keys.Count != 1)
                throw new ApplicationException("Expected only one arguments with key");
            if(!args[1].StartsWith(KeySign))
                throw new ApplicationException("Arguments list must starts from key");
            CommandInfo info = new CommandInfo();
            FillCommandType(info, keys[0]);
            foreach (String keyValuePair in keyValuePairs)
                ReadKeyValueOption(info, keyValuePair);
            return info;
        }

        private static void FillCommandType(CommandInfo info, String command)
        {
            String commandLowerCase = command.ToLower();

            if (String.Equals(commandLowerCase, CommandsKeys.StartCommandKey))
                info.Command = CommandType.Start;
            else if (String.Equals(commandLowerCase, CommandsKeys.StopCommandKey))
                info.Command = CommandType.Stop;
            else if (String.Equals(commandLowerCase, CommandsKeys.RestartCommandKey))
                info.Command = CommandType.Restart;
            else if (String.Equals(commandLowerCase, CommandsKeys.HelpCommandKey))
                info.Command = CommandType.Help;
            else if (String.Equals(commandLowerCase, CommandsKeys.QuitCommandKey))
                info.Command = CommandType.Quit;
            else throw new ApplicationException("Unexpected command");
        }

        private static void ReadKeyValueOption(CommandInfo info, String keyValue)
        {
            String keyValueLower = keyValue.ToLower().Trim();
            if(!keyValueLower.Contains(KeyValueSeparator))
                throw new ApplicationException(String.Format("Invalid data, expected separator \'{0}\' between key and value", KeyValueSeparator));
            if (keyValueLower.StartsWith(CommandsOptions.IpAddressKey))
                info.IpAddress = Getvalue(keyValueLower);
            else if (keyValueLower.StartsWith(CommandsOptions.PortKey))
                info.Port = UInt16.Parse(Getvalue(keyValueLower));
            else if (keyValueLower.StartsWith(CommandsOptions.ScriptKey))
                info.ScriptFile = Getvalue(keyValueLower);
            else if (keyValueLower.StartsWith(CommandsOptions.ServerSettingsKey))
                info.SettingsFile = Getvalue(keyValueLower);
            else throw new ApplicationException("Invalid data, unexpected data key");
        }

        private static String Getvalue(String keyValue)
        {
            Int32 startIndex = keyValue.IndexOf(KeyValueSeparator, StringComparison.InvariantCulture) + 1;
            if(startIndex >= keyValue.Length - 1)
                throw new ApplicationException("Invalid data, no value after separator");
            return keyValue.Substring(startIndex);
        }

        private const String KeySign = "--";
        private const String KeyValueSeparator = "=";
    }
}
