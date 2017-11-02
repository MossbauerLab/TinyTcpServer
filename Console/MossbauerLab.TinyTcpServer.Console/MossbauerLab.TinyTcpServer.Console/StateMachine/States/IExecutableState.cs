using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine.States
{
    public delegate TR ExecuteFunc<TE, TR>(ref TE executer, Object[] args);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TE"> Is a executer</typeparam>
    /// <typeparam name="TS"> Is a state </typeparam>
    /// <typeparam name="TR"> Is a result of execute operation </typeparam>
    public interface IExecutableState<TE, TS, TR>
    {
        TR Execute(ExecuteFunc<TE, TR> executerFunc, ref TE executer, Object[] args);
        TS GetState();
    }
}
