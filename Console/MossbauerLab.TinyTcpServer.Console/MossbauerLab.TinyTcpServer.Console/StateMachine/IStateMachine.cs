using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public interface IStateMachine
    {
        Boolean AddState<T, R>(Func<T, R, Boolean> transitionChecker, Guid stateId, T newState) where T: IComparable; // for Mealy state machine : T is state, R is input
        Boolean AddState<T>(Func<T, Boolean> transitionChecker, Guid stateId, T newState) where T : IComparable;      // for Moore state machine : T is state, R is input
        Boolean RemoveState(Guid stateId);
        void Run(Boolean termonate);
    }
}
