using System;

namespace MossbauerLab.TinyTcpServer.MnGUI.Data
{
    [Flags]
    public enum ServerType
    {
        Echo,
        Time,
        Scripting     // this option is for Server specifying with script
    }
}
