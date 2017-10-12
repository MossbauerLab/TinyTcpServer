using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public interface IExecutableState : IComparable
    {
        TR Execute<T, TR>();
        T State<T>();
    }
}
