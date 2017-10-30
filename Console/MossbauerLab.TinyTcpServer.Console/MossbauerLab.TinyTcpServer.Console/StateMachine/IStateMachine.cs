using System;
using System.Text;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public interface IStateMachine<TS, TI> 
        where TS: IComparable 
        where TI: class
    {
        Boolean Add(Guid checkerId, Func<TS, TS, Object[], Boolean> transitionChecker);            // for Mealy state machine : TS is state, TI is input
        Boolean Remove(Guid checkerId);
        void Run(ref Boolean terminate, TI input);
    }
}
