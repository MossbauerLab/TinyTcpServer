using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public interface IStateMachine<TS, TI> 
        where TS: IComparable 
        where TI: class
    {
        Boolean Add(Func<TS, TI, Boolean> transitionChecker, Guid stateId, TS newState, TI input);            // for Mealy state machine : TS is state, TI is input
        Boolean Add(Func<TS, Boolean> transitionChecker, Guid stateId, TS newState);                          // for Moore state machine : TS is state, TI is input
        Boolean Remove(Guid stateId);
        void Run(Boolean termonate);
    }
}
