using System;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public abstract class MealyMachine<TS, TI> : IStateMachine<TS, TI>
        where TS : IComparable
        where TI : class
    {
        public abstract Boolean Add(Guid checkerId, Func<TS, TS, Object[], Boolean> transitionChecker);

        public abstract Boolean Remove(Guid checkerId);

        public abstract void Run(ref Boolean terminate, TI input);
    }
}
