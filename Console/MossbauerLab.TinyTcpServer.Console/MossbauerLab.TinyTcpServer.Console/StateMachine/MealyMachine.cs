using System;
using MossbauerLab.TinyTcpServer.Console.StateMachine.States;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public abstract class MealyMachine : IStateMachine<MachineState, String>
    {
        public abstract Boolean Add(Func<MachineState, String, Boolean> transitionChecker, Guid stateId, MachineState newState, String input);

        public bool Add(Func<MachineState, Boolean> transitionChecker, Guid stateId, MachineState newState)
        {
            throw new InvalidOperationException("This operation is for Moore machine only");
        }

        public abstract Boolean Remove(Guid stateId);

        public abstract void Run(Boolean termonate);
    }
}
