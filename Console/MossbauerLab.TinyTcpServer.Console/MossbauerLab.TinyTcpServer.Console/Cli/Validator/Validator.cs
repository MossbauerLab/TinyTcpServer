using System;
using MossbauerLab.TinyTcpServer.Console.Cli.Data;
using MossbauerLab.TinyTcpServer.Console.Cli.Options;

namespace MossbauerLab.TinyTcpServer.Console.Cli.Validator
{
    public static class Validator
    {
        public static Boolean Validate(CommandInfo info, Boolean inited = false)
        {
            if (info.Command == CommandType.Stop)
                return CheckStopInfo(info, inited);
            if (info.Command == CommandType.Start)
                return CheckStartInfo(info, inited);
            if (info.Command == CommandType.Restart)
                return CheckRestartInfo(info, inited);
            if ((info.Command == CommandType.Help) || (info.Command == CommandType.Quit))
                return CheckAllFieldsAreNull(info);
            return false;
        }

        private static Boolean CheckStartInfo(CommandInfo info, Boolean inited)
        {
            // only script is obligatory if not inited
            if (!inited)
                return info.ScriptFile != null;
            return info.ScriptFile == null && info.SettingsFile == null;
        }

        private static Boolean CheckStopInfo(CommandInfo info, Boolean inited)
        {
            // we are not expecting any other argument here
            return CheckAllFieldsAreNull(info) && inited;
        }

        private static Boolean CheckRestartInfo(CommandInfo info, Boolean inited)
        {
            return info.ScriptFile == null && info.SettingsFile == null && inited;
        }

        private static Boolean CheckAllFieldsAreNull(CommandInfo info)
        {
            return info.IpAddress == null && info.Port == null && info.ScriptFile == null && info.SettingsFile == null;
        }
    }
}
