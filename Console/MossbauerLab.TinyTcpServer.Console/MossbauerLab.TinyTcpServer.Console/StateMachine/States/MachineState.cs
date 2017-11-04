using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine.States
{
    [Flags]
    public enum MachineState
    {
        Initial,
        Initialized,
        Started,
        Stopped
    }
}
