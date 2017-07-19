using System;
using System.Collections.Generic;
using MossbauerLab.TinyTcpServer.Core.Client;
using MossbauerLab.TinyTcpServer.Core.Handlers;

namespace MossbauerLab.TinyTcpServer.Core.Server
{
    public interface ITcpServer
    {
        Boolean Start();
        Boolean Start(String ipAddress, UInt16 port);
        void Stop(Boolean clearHandlers);
        void Restart();
        // handler functions
        void AddHandler(TcpClientHandlerInfo clientHandlerInfo, Func<Byte[], TcpClientHandlerInfo, Byte[]> handler);
        void RemoveHandler(TcpClientHandlerInfo clientHandlerInfo);
        // send data functions
        void SendData(TcpClientHandlerInfo clientHandlerInfo, Byte[] data);
        // filtering functions
        // properties
        Int32 ConnectedClients { get; }
        Boolean IsReady { get; }
        IList<TcpClientContext> Clients { get; }
        void DisconnectAllClients();
    }
}