using System;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine.States
{
    public class TcpServerState : IExecutableState<ITcpServer, MachineState, Boolean>
    {
        public TcpServerState(MachineState state)
        {
            _state = state;
        }

        public Boolean Execute(ExecuteFunc<ITcpServer, Boolean> executerFunc, ref ITcpServer executer, Object[] args)
        {
            return executerFunc(ref executer, args);
        }

        public MachineState GetState()
        {
            return _state;
        }

        private readonly MachineState _state;
    }
}
