using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public interface IStateMachine<TS, TI, TR> 
        where TS: IComparable 
        where TI: class
    {
        Boolean AddState(TS state);
        Boolean RemoveState(TS state);
        Boolean SetTransitions(Func<TS, TI, TR> transitions);     // Mealy machine
        Boolean SetTransitions(Func<TS, TR> transitions);         // Moore machine
        Boolean Run(TI input);
    }
}
