using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine.States
{
    public interface IExecutableState<T, TS, TR> : IComparable
    {
        Boolean Execute(Func<T, TS, TR> executer, Object[] args);
        TS GetState();
    }
}
