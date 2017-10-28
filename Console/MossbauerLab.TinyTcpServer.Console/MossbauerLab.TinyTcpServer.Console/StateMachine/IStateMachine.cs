using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public interface IStateMachine
    {
        Boolean Add<TS, TI>(Func<TS, TI, Boolean> transitionChecker, Guid stateId, 
                                 TS newState, TI input) where TS: IComparable;                                         // for Mealy state machine : TS is state, TI is input
        Boolean Add<TS>(Func<TS, Boolean> transitionChecker, Guid stateId, TS newState) where TS : IComparable;   // for Moore state machine : TS is state, TI is input
        Boolean Remove(Guid stateId);
        void Run(Boolean termonate);
    }
}
