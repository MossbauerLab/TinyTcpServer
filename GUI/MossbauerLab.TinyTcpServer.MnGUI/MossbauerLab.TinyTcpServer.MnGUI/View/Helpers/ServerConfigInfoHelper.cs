using System;
using System.Collections.Generic;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.TinyTcpServer.MnGUI.View.Helpers
{
    public static class ServerConfigInfoHelper
    {
        public static IList<String> GetConfigStrings(TcpServerConfig config)
        {
            IList<String> strings = new List<String>();
            strings.Add(String.Format(ConnectTimeoutTemplate, config.ClientConnectTimeout));
            strings.Add(String.Format(ReadTimeoutTemplate, config.ReadTimeout));
            strings.Add(String.Format(WriteTimeoutTemplate, config.WriteTimeout));
            strings.Add(String.Format(ServerCloseTimeoutTemplate, config.ServerCloseTimeout));
            strings.Add(String.Format(ClientInactivityTimeTemplate, config.ClientInactivityTime));

            strings.Add(String.Format(ConnectAttemptsTemplate, config.ClientConnectAttempts));
            strings.Add(String.Format(ReadAttemptsTemplate, config.ClientReadAttempts));

            strings.Add(String.Format(ParallelTasksTemplate, config.ParallelTask));
            strings.Add(String.Format(ClientBufferSizeTemplate, config.ClientBufferSize));
            return strings;
        }

        private const String ConnectTimeoutTemplate = "Connect timeout: {0} ms";
        private const String ReadTimeoutTemplate = "Read timeout: {0} ms";
        private const String WriteTimeoutTemplate = "Write timeout: {0} ms";
        private const String ServerCloseTimeoutTemplate = "Server close timeout: {0} ms";
        private const String ClientInactivityTimeTemplate = "Client inactivity time: {0} s";

        private const String ConnectAttemptsTemplate = "Connect attempts: {0} times";
        private const String ReadAttemptsTemplate = "Read attempts: {0} times";

        private const String ParallelTasksTemplate = "Parallel tasks: {0}";
        private const String ClientBufferSizeTemplate = "Client buffer size: {0} bytes";
    }
}
