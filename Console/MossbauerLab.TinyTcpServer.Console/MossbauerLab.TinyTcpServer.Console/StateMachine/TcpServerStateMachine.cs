using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MossbauerLab.TinyTcpServer.Console.StateMachine.States;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public class TcpServerStateMachine : MealyMachine<TcpServerState, StringBuilder>
    {
        public TcpServerStateMachine(ITcpServer server)
        {
            if(server == null)
                throw new ArgumentNullException("server");
            _server = server;
            _states.Add(new TcpServerState(MachineState.Initial));
            _states.Add(new TcpServerState(MachineState.Started));
            _states.Add(new TcpServerState(MachineState.Stopped));
        }

        public override Boolean Add(Guid checkerId, Func<TcpServerState, TcpServerState, Object[], Boolean> transitionChecker)
        {
            _transitionCheckers[checkerId] = transitionChecker;
        }

        public override Boolean Remove(Guid checkerId)
        {
            if (!_transitionCheckers.ContainsKey(checkerId))
                return false;
            _transitionCheckers.Remove(checkerId);
            return true;
        }

        public override void Run(ref Boolean terminate, StringBuilder input)
        {
            if (terminate)
            {
                _server.Stop(true);
                return;
            }
            //_transitionCheckers.Where()
        }

        private Boolean ExecuteStartState(ITcpServer server, MachineState state, Object[] args)
        {
            Boolean result;
            if (args.Length == 2)
            {
                String ipAddress = args[0] as String;
                UInt16 port = (UInt16) args[1];
                result = server.Start(ipAddress, port);
                _currentState = result ? MachineState.Started : _currentState;
                return result;
            }
            result = server.Start();
            _currentState = result ? MachineState.Started : _currentState;
            return result;
        }

        private Boolean ExecuteStopState(ITcpServer server, MachineState state, Object[] args)
        {
            server.Stop(true);
            _currentState = MachineState.Stopped;
            return true;
        }

        private ITcpServer _server;
        private MachineState _currentState = MachineState.Initial;
        private readonly IList<IExecutableState<ITcpServer, MachineState, Boolean>> _states = new List<IExecutableState<ITcpServer, MachineState, Boolean>>();

        private readonly IDictionary<Guid, Func<TcpServerState, TcpServerState, Object[], Boolean>> _transitionCheckers =
                new Dictionary<Guid, Func<TcpServerState, TcpServerState, Object[], Boolean>>();
    }
}
