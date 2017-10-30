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

        public Int32 CompareTo(Object obj)
        {
            TcpServerState comparingState = obj as TcpServerState;
            if (comparingState == null)
                throw new ApplicationException("comparing state can't be null");
            if (comparingState.GetState() < _state)
                return 1;
            if (comparingState.GetState() == _state)
                return 0;
            return -1;
        }

        public Boolean Execute(Func<ITcpServer, MachineState, Object[], Boolean> executer, Object[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("args must contains at least 2 elements");
            Object[] additionalParams = new Object[args.Length - 2];
            if (args.Length > 2)
                Array.Copy(args, 2, additionalParams, 0, additionalParams.Length);
            return executer((ITcpServer) args[0], (MachineState) args[1], additionalParams);
        }

        public MachineState GetState()
        {
            return _state;
        }

        private readonly MachineState _state;
    }
}
