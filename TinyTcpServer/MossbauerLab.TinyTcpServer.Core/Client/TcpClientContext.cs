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
            Id = Guid.NewGuid();
            Client = client;
            BytesRead = 0;
            ReadDataEvent = new ManualResetEventSlim(false, 100);
            WriteDataEvent = new ManualResetEventSlim(false, 100);
            IsProcessing = false;
            SynchObject = new Object();

        }

        public Guid Id { get; set; }
        public Object SynchObject { get; set; }
        public Boolean IsProcessing { get; set; }
        public Int32 BytesRead { get; set; }
        public TcpClient Client { get; private set; }
        public ManualResetEventSlim ReadDataEvent { get; set; }
        public ManualResetEventSlim WriteDataEvent { get; set; }
        public Boolean Inactive { get; set; }
        public DateTime InactiveTimeMark { get; set; }
    }
}
