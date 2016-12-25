using System;
using System.Net;
using System.Net.Sockets;
using TinyTcpServer.Core.Client;

namespace TinyTcpServer.Core.Handlers.Utils
{
    internal static class TcpClientHandlerSelector
    {
        public static Boolean Select(TcpClientHandlerInfo clientHandlerInfo, TcpClientContext tcpClient)
        {
            if (String.Equals(GlobalDefs.AnyIpAddress, clientHandlerInfo.IpAddress) && GlobalDefs.AnyPort == clientHandlerInfo.Port)
                return true;

            return SelectByIpAddressComparison(clientHandlerInfo.IpAddress, tcpClient.Client) &&
                   SelectByPortComparison(clientHandlerInfo.Port, tcpClient.Client);
        }

        private static Boolean SelectByIpAddressComparison(String clientHandlerIpAddress, TcpClient tcpClient)
        {
            return String.Equals(GlobalDefs.AnyIpAddress, clientHandlerIpAddress) ||
                   String.Equals(((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address.ToString(), clientHandlerIpAddress);
        }

        private static Boolean SelectByPortComparison(Int32 clientHandlerPort, TcpClient tcpClient)
        {
            return GlobalDefs.AnyPort == clientHandlerPort ||
                   ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Port == clientHandlerPort;
        }
    }
}
