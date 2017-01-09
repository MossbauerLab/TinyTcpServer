using System;
using MossbauerLab.TinyTcpServer.Core.Handlers;

namespace MossbauerLab.TinyTcpServer.Core.Server
{
    public interface ITcpServer
    {
        Boolean Start(String ipAddress, UInt16 port);
        void Stop(Boolean clearHandlers);
        void Restart();
        // handler functions
        void AddHandler(TcpClientHandlerInfo clientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]> handler);
        void RemoveHandler(TcpClientHandlerInfo clientHandlerInfo);
        // send data functions
        void SendData(TcpClientHandlerInfo clientHandlerInfo, Byte[] data);
        // filtering functions
    }
}