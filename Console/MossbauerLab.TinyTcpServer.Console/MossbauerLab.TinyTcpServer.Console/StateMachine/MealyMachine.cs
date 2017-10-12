using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public abstract class MealyMachine : IStateMachine
    {
        public abstract bool AddState<TS, TI>(Func<TS, TI, Boolean> transitionChecker, Guid stateId, TS newState, TI input) where TS : IComparable;

        public bool AddState<TS>(Func<TS, Boolean> transitionChecker, Guid stateId, TS newState) where TS : IComparable
        {
            throw new InvalidOperationException("This operation is for Moore machine only");
        }

        public abstract bool RemoveState(Guid stateId);

        public abstract void Run(Boolean termonate);
    }
}
