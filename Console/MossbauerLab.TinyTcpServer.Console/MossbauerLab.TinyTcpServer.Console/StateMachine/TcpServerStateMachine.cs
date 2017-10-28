using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public class TcpServerStateMachine : MealyMachine
    {
        public override bool Add<TS, TI>(Func<TS, TI, bool> transitionChecker, Guid stateId, TS newState, TI input)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(Guid stateId)
        {
            throw new NotImplementedException();
        }

        public override void Run(bool termonate)
        {
            throw new NotImplementedException();
        }
    }
}
