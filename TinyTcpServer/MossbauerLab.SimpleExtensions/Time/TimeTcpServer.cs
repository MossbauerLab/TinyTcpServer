using System;
using System.Threading.Tasks;
using log4net;
using MossbauerLab.TinyTcpServer.Core.Handlers;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.SimpleExtensions.Time
{
    /// <summary>
    ///  RFC 868 Implementation
    /// </summary>
    public class TimeTcpServer : TcpServer
    {
        public TimeTcpServer(String ipAddress, UInt16 port = DefaultTimePort, ILog logger = null, Boolean debug = false, TcpServerConfig config = null)
            : base(ipAddress, port, logger, debug)
        {
            _task = new Task(ClientProccessor);
        }

        public override Boolean Start()
        {
            Boolean result = base.Start();
            _task.Start();
            return result;
        }

        public override void Stop(Boolean clearHandlers)
        {
            _stopRequested = true;
            base.Stop(clearHandlers);
        }

        private void ClientProccessor()
        {
            while (!_stopRequested)
            {
                if (ConnectedClients > 0)
                {
                    Int32 totalSeconds = (Int32)Math.Floor(DateTime.Now.Subtract(_origin).TotalSeconds);
                    Byte[] bytes =
                    {
                        (Byte) (totalSeconds & 0x000000FF), (Byte) ((totalSeconds & 0x0000FF00) >> 8),
                        (Byte) ((totalSeconds & 0x00FF0000) >> 16), (Byte) ((totalSeconds & 0xFF000000) >> 24)
                    };
                    SendData(new TcpClientHandlerInfo(Guid.NewGuid()), bytes);
                    DisconnectAllClients();
                }
            }
        }

        private const UInt16 DefaultTimePort = 37;
        private DateTime _origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly Task _task;
        private Boolean _stopRequested;
    }
}
