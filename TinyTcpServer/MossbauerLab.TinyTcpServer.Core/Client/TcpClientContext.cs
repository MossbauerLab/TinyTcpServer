using System;
using System.Net.Sockets;
using System.Threading;

namespace MossbauerLab.TinyTcpServer.Core.Client
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
            SynchObject = new Object();
        }

        public Object SynchObject { get; set; }
        public Boolean IsProcessing { get; set; }
        public Int32 BytesRead { get; set; }
        public TcpClient Client { get; private set; }
        public ManualResetEventSlim ReadDataEvent { get; set; }
        public ManualResetEventSlim WriteDataEvent { get; set; }
    }
}
