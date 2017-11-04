using System;
using System.Collections.Generic;
using System.Linq;
using MossbauerLab.TinyTcpServer.Console.Cli.Data;
using MossbauerLab.TinyTcpServer.Console.Cli.Options;

namespace MossbauerLab.TinyTcpServer.Console.cli.Parser
{
    public static class Parser
    {
        public static CommandInfo Parse(String[] args)
        {
            if(args == null)
                throw new NullReferenceException("args");
            IList<String> keys = args.Where(line => line.StartsWith(KeySign)).ToList();
            IList<String> keyValuePairs = args.Where(line => line.Contains(KeyValueSeparator)).ToList();
            if (keys.Count == 0)
                throw new ApplicationException("Arguments list doesn't contains keys");
            CommandInfo info = new CommandInfo();
            FillCommandType(info, keys[0]);
            foreach (String keyValuePair in keyValuePairs)
                ReadKeyValueOption(info, keyValuePair);
            return info;
        }

        private static void FillCommandType(CommandInfo info, String command)
        {
            String commandLowerCase = command.ToLower().Trim();
            if(!Operations.ContainsKey(commandLowerCase))
                throw new ApplicationException("Unexpected command");
            info.Command = Operations[commandLowerCase];
        }

        private static void ReadKeyValueOption(CommandInfo info, String keyValue)
        {
            Int32 keyLength = keyValue.IndexOf(KeyValueSeparator, StringComparison.InvariantCulture);
            String key = keyValue.Substring(0, keyLength).ToLower().Trim(TrimmingSymbols);
            if(!key.StartsWith(KeySign))
                throw new ApplicationException(String.Format("Key must begin from separator {0} symbols", KeySign));
            key = key.Trim(KeySign[0]);
            if (key.Equals(CommandsOptions.IpAddressKey))
                info.IpAddress = GetValue(keyValue);
            else if (key.Equals(CommandsOptions.PortKey))
                info.Port = UInt16.Parse(GetValue(keyValue));
            else if (key.Equals(CommandsOptions.ScriptKey))
                info.ScriptFile = GetValue(keyValue);
            else if (key.Equals(CommandsOptions.ServerSettingsKey))
                info.SettingsFile = GetValue(keyValue);
            else throw new ApplicationException("Invalid data, unexpected data key");
        }

        private static String GetValue(String key)
        {
            Int32 startIndex = key.IndexOf(KeyValueSeparator, StringComparison.InvariantCulture) + 1;
            if(startIndex >= key.Length - 1)
                throw new ApplicationException("Invalid data, no value after separator");
            String value = key.Substring(startIndex);
            return value;
        }

        private const String KeySign = "--";
        private const String KeyValueSeparator = "=";

        private static readonly Char[] TrimmingSymbols = {' ', '\t', '\n', '\r'};

        private static readonly IDictionary<String, CommandType> Operations = new Dictionary<String, CommandType>()
        {
            {Command.StartCommandKey, CommandType.Start},
            {Command.StopCommandKey, CommandType.Stop},
            {Command.RestartCommandKey, CommandType.Restart},
            {Command.QuitCommandKey, CommandType.Quit},
            {Command.HelpCommandKey, CommandType.Help}
        };
    }
}
