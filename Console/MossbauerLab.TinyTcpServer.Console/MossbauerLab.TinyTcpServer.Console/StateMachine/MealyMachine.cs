using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public abstract class MealyMachine<TS, TI, TR> : IStateMachine<TS, TI, TR>
        where TS : IComparable
        where TI : class
    {
        public abstract Boolean AddState(TS state);
        public abstract Boolean RemoveState(TS state);
        public abstract Boolean SetTransitions(Func<TS, TI, TR> transitions);     // Mealy machine

        public Boolean SetTransitions(Func<TS, TR> transitions)
        {
            throw new InvalidOperationException("This operation is not valid for Melay machine");
        }

        public abstract Boolean Run(TI input);
    }
}
