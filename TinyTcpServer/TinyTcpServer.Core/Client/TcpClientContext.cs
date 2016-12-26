using System;
using System.Net.Sockets;
using System.Threading;

namespace TinyTcpServer.Core.Client
{
    //todo: umv: add dispose
    public class TcpClientContext
    {
        public TcpClientContext(TcpClient client)
        {
            if(client == null)
                throw new ArgumentNullException("client");
            Client = client;
            BytesRead = 0;
            ReadDataEvent = new ManualResetEventSlim();
            WriteDataEvent = new ManualResetEventSlim();
            IsProcessing = false;
        }

        public Boolean IsProcessing { get; set; }
        public Int32 BytesRead { get; set; }
        public TcpClient Client { get; private set; }
        public ManualResetEventSlim ReadDataEvent { get; set; }
        public ManualResetEventSlim WriteDataEvent { get; set; }
    }
}
