using System;
using TinyTcpServer.Api.Client;

namespace TinyTcpServer.Api.Server
{
    public interface ITcpServer
    {
        Boolean Start(String ipAddress, UInt16 port);
        void Stop();
        void Restart();
        // handler functions
        void AddHandler(TcpClientInfo clientInfo, Func<Byte[], TcpClientInfo, Byte[]> handler);
        void RemoveHandler(TcpClientInfo clientInfo);
        // send data functions
        void SendData(TcpClientInfo clientInfo, Byte[] data);
        // filtering functions
    }
}