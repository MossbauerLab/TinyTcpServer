using System;

namespace MossbauerLab.TinyTcpServer.Core.Server
{
    public class TcpServerConfig
    {
        public TcpServerConfig()
        {
            Init();
        }

        public TcpServerConfig(Int32 clientConnectTimeout = DefaultClientConnectTimeout, Int32 readTimeout = DefaultReadTimeout,
                               Int32 writeTimeout = DefaultWriteTimeout, Int32 serverCloseTimeout = DefaultServerCloseTimeout, 
                               Int32 clientInactivityTime = DefaultClientInactiveTime, Int32 clientConnectAttempts = DefaultClientConnectAttempts, 
                               Int32 clietnReadAttempts = DefaultReadAttempts, Int32 parallelTasks = DefaultParallelTasks,
                               Int32 clientBufferSize = DefaultClientBufferSize, Int32 chunkSize = DefaultChunkSize)
        {
            Init(clientConnectTimeout, readTimeout, writeTimeout, serverCloseTimeout, clientInactivityTime, clientConnectAttempts, 
                 clietnReadAttempts, parallelTasks, clientBufferSize, chunkSize);
        }

        private void Init(Int32 clientConnectTimeout = DefaultClientConnectTimeout, Int32 readTimeout = DefaultReadTimeout,
                          Int32 writeTimeout = DefaultWriteTimeout, Int32 serverCloseTimeout = DefaultServerCloseTimeout, 
                          Int32 clientInactivityTime = DefaultClientInactiveTime, Int32 clientConnectAttempts = DefaultClientConnectAttempts, 
                          Int32 clietnReadAttempts = DefaultReadAttempts, Int32 parallelTasks = DefaultParallelTasks,
                          Int32 clientBufferSize = DefaultClientBufferSize, Int32 chunkSize = DefaultChunkSize)
        {
            ClientConnectTimeout = clientConnectTimeout;
            ReadTimeout = readTimeout;
            WriteTimeout = writeTimeout;
            ServerCloseTimeout = serverCloseTimeout;
            ClientInactivityTime = clientInactivityTime;
            ClientConnectAttempts = clientConnectAttempts;
            ClientReadAttempts = clietnReadAttempts;
            ParallelTask = parallelTasks;
            ClientBufferSize = clientBufferSize;
            ChunkSize = chunkSize;
        }

        public Int32 ClientConnectTimeout { get; set; }
        public Int32 ReadTimeout { get; set; }
        public Int32 WriteTimeout { get; set; }
        public Int32 ServerCloseTimeout { get; set; }
        public Int32 ClientInactivityTime { get; set; }
        public Int32 ClientConnectAttempts { get; set; }
        public Int32 ClientReadAttempts { get; set; }

        public Int32 ParallelTask { get; set; }

        public Int32 ClientBufferSize { get; set; }
        public Int32 ChunkSize { get; set; }


        private const Int32 DefaultServerCloseTimeout = 2000;
        private const Int32 DefaultClientBufferSize = 16384;
        private const Int32 DefaultChunkSize = 8192;
        private const Int32 DefaultClientConnectAttempts = 1;
        private const Int32 DefaultClientConnectTimeout = 50;     //ms
        private const Int32 DefaultReadTimeout = 1000;            //ms
        private const Int32 DefaultWriteTimeout = 1000;           //ms
        private const Int32 DefaultReadAttempts = 8;
        private const Int32 DefaultParallelTasks = 128;
        private const Int32 DefaultClientInactiveTime = 120;      //s (seconds)
    }
}
