using System;
using System.Net.Sockets;
using System.Threading;

namespace MossbauerLab.TinyTcpServer.Core.FunctionalTests.TestUtils
{
    public class TransportClient
    {
        public TransportClient(Boolean isAsync, String server, UInt16 port)
        {
            Init(server, port, isAsync);
            _client = new TcpClient();
        }

        public Boolean Open(String server, UInt16 port, Boolean isAsync = true)
        {
            Init(server, port, isAsync);
            return Open();
        }

        public Boolean Open()
        {
            if(_isAsync)
                OpenAsync();
            else OpenSync();
            return _client.Connected;
        }

        public void Close()
        { 
            _client.Close();
        }

        public Boolean Read(Byte[] data, out Int32 bytesRead)
        {
            bytesRead = 0;
            return false;
        }

        public Boolean Write(Byte[] data)
        {
            return false;
        }

        private void OpenSync()
        {
            _client.Connect(_server, _port);
        }

        private void OpenAsync()
        {
            _connectCompleted.Reset();
            _client.BeginConnect(_server, _port, OpenAsyncCallback, _client);
            _connectCompleted.Wait(DefaultConnectTimeout);
        }

        private void Init(String server, UInt16 port, Boolean isAsync)
        {
            if (String.IsNullOrEmpty(server))
                throw new ArgumentNullException("server");
            _isAsync = isAsync;
            _server = server;
            _port = port;
        }

        private void OpenAsyncCallback(IAsyncResult result)
        {
            TcpClient client = (result.AsyncState as TcpClient);
            if (client == null)
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("client");
            client.EndConnect(result);
            _connectCompleted.Set();
        }

        private const Int32 DefaultConnectTimeout = 1000;

        private Boolean _isAsync;
        private String _server;
        private UInt16 _port;
        private readonly TcpClient _client;

        private readonly ManualResetEventSlim _connectCompleted = new ManualResetEventSlim(false);
    }
}
