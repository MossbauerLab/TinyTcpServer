using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.TinyTcpServer.Console.Builders
{
    public static class TcpServerConfigBuilder
    {
        public static TcpServerConfig Build(String serverConfig)
        {
            if(String.IsNullOrEmpty(serverConfig))
                throw new ArgumentNullException("serverConfig");
            if(!File.Exists(serverConfig))
                throw new ApplicationException("Config file does not exists");
            IList<String> content = File.ReadAllLines(serverConfig).Select(line=>line.Trim().ToLower())
                                                                   .Where(line => !String.IsNullOrEmpty(line))
                                                                   .Where(line => line.StartsWith(CommentarySymbol))
                                                                   .ToList();
            TcpServerConfig config = new TcpServerConfig();
            Int32 value = GetConfigurationValue(content, ParallelTaskKey);
            if (value != -1)                     // otherwise we using default value
                config.ParallelTask = value;
            value = GetConfigurationValue(content, ClientBufferSizeKey);
            if (value != -1)                     // otherwise we using default value
                config.ClientBufferSize = value;
            value = GetConfigurationValue(content, ChunkSizeKey);
            if (value != -1)                     // otherwise we using default value
                config.ChunkSize = value;
            value = GetConfigurationValue(content, ClientConnectAttemptsKey);
            if (value != -1)                     // otherwise we using default value
                config.ClientConnectAttempts = value;
            value = GetConfigurationValue(content, ClientConnectTimeoutKey);
            if (value != -1)                     // otherwise we using default value
                config.ClientConnectTimeout = value;
            value = GetConfigurationValue(content, ClientInactivityTimeKey);
            if (value != -1)                     // otherwise we using default value
                config.ClientInactivityTime = value;
            value = GetConfigurationValue(content, ReadTimeoutKey);
            if (value != -1)                     // otherwise we using default value
                config.ReadTimeout = value;
            value = GetConfigurationValue(content, WriteTimeoutKey);
            if (value != -1)                     // otherwise we using default value
                config.WriteTimeout = value;
            value = GetConfigurationValue(content, ClientReadAttemptsKey);
            if (value != -1)                     // otherwise we using default value
                config.ClientReadAttempts= value;
            value = GetConfigurationValue(content, ServerCloseTimeoutKey);
            if (value != -1)                     // otherwise we using default value
                config.ServerCloseTimeout = value;
            return config;
        }

        private static Int32 GetConfigurationValue(IList<String> fileContent, String key)
        {
            try
            {
                String configLine = fileContent.FirstOrDefault(line => line.StartsWith(key.ToLower()));
                if (configLine == null)
                    return -1;
                Int32 index = configLine.IndexOf(KeyValueSeparator, StringComparison.InvariantCulture);
                if (index <= 0)
                    return -1;
                String value = configLine.Substring(index + 1);
                return Int32.Parse(value);
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private const String KeyValueSeparator = ":";
        private const String CommentarySymbol = "#";

        private const String ParallelTaskKey = "ParallelTask";
        private const String ClientBufferSizeKey = "ClientBufferSize";
        private const String ChunkSizeKey = "ChunkSize";
        private const String ClientConnectAttemptsKey = "ClientConnectAttempts";
        private const String ClientInactivityTimeKey = "ClientInactivityTime";
        private const String ClientConnectTimeoutKey = "ClientConnectTimeout";
        private const String ClientReadAttemptsKey = "ClientReadAttempts";
        private const String ReadTimeoutKey = "ReadTimeout";
        private const String ServerCloseTimeoutKey = "ServerCloseTimeout";
        private const String WriteTimeoutKey = "WriteTimeout";
    }
}
