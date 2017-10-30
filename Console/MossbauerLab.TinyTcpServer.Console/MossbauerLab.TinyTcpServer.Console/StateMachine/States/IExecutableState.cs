using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine.States
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"> Is a tcp server </typeparam>
    /// <typeparam name="TS"> Is a state </typeparam>
    /// <typeparam name="TR"> Is a result of execute operation </typeparam>
    public interface IExecutableState<T, TS, TR> : IComparable
    {
        Boolean Execute(Func<T, TS, Object[], TR> executer, Object[] args);
        TS GetState();
    }
}
