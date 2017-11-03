using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using MossbauerLab.TinyTcpServer.Console.Builders;
using MossbauerLab.TinyTcpServer.Console.cli.Parser;
using MossbauerLab.TinyTcpServer.Console.Cli.Data;
using MossbauerLab.TinyTcpServer.Console.Cli.Options;
using MossbauerLab.TinyTcpServer.Console.Cli.Validator;
using MossbauerLab.TinyTcpServer.Console.StateMachine.States;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.TinyTcpServer.Console.StateMachine
{
    public  class TcpServerMachineTransition
    {
        public TcpServerMachineTransition(Boolean isvalid, Boolean stopMachine, 
                                          IList<Tuple<TcpServerState, Object[]>>  statesSequence)
        {
            IsValid = isvalid;
            StopMachine = stopMachine;
            StatesSequence = statesSequence;
        }

        public Boolean IsValid { get; private set; }
        public Boolean StopMachine { get; private set; }
        public IList<Tuple<TcpServerState, Object[]>> StatesSequence { get; private set; }
    }

    public class TcpServerStateMachine : MealyMachine <MachineState, StringBuilder, TcpServerMachineTransition>
    {
        public TcpServerStateMachine(ITcpServer server, ILog logger)
        {
            //if(server == null)
                //throw new ArgumentNullException("server");
            _server = server;
            _logger = logger;
        }

        public void DefaultInit()
        {
            AddState(MachineState.Initial);
            AddState(MachineState.Started);
            AddState(MachineState.Stopped);
            SetTransitions(DefaultTransitions);
        }

        public override Boolean AddState(MachineState state)
        {
            if (_states.Any(item => item == state))
                return false;
            _states.Add(state);
            return true;
        }

        public override Boolean RemoveState(MachineState state)
        {
            if (!_states.Contains(state))
                return false;
            _states.Remove(state);
            return true;
        }

        public override Boolean SetTransitions(Func<MachineState, StringBuilder, TcpServerMachineTransition> transitions)
        {
            _transitionFunc = transitions;
            return true;
        }

        public override Boolean Run(StringBuilder input)
        {
            TcpServerMachineTransition transition = _transitionFunc(_currentState, input);
            if (transition.StopMachine)
            {
                // quit
                System.Console.WriteLine("Exiting from tcp server console");
                return false;
            }
            if (!transition.IsValid)
            {
                // not valid commands, notify user ...
                System.Console.WriteLine("Invalid command or argumemnts, see --help");
                return true;
            }
            if (transition.StatesSequence == null)
            {
                // display help
                System.Console.WriteLine("This is help");
            }
            else
            {
                foreach (Tuple<TcpServerState, Object[]> state in transition.StatesSequence)
                {
                    if (state.Item1.GetState() == MachineState.Started)
                        state.Item1.Execute(ExecuteStartState, ref _server, state.Item2);
                    if (state.Item1.GetState() == MachineState.Stopped)
                        state.Item1.Execute(ExecuteStopState, ref _server, state.Item2);
                }
            }

            return true;
        }

        private Boolean ExecuteStartState(ref ITcpServer server, Object[] args)
        {
            const String serverStartFormat = "=================> Server was started on {0} : {1}";
            Boolean result;
            if (args.Length >= 2)
            {
                String ipAddress = args[0] as String;
                UInt16 port = (UInt16) args[1];
                String settingsFile = args[2] as String;
                String scriptFile = args[3] as String;
                TcpServerConfig config = settingsFile != null ? TcpServerConfigBuilder.Build(settingsFile) : null;
                if(server == null || scriptFile != null)
                    server = new FlexibleTcpServer(scriptFile, ipAddress, port, _logger, false, config);
                result = server.Start(ipAddress, port);
                _currentState = result ? MachineState.Started : _currentState;
                System.Console.WriteLine(serverStartFormat, ipAddress, port);
                System.Console.WriteLine("=================> Server was started");
                return result;
            }
            result = server.Start();
            _currentState = result ? MachineState.Started : _currentState;
            return result;
        }

        private Boolean ExecuteStopState(ref ITcpServer server, Object[] args)
        {
            if (server == null)
                return false;
            server.Stop(true);
            _currentState = MachineState.Stopped;
            System.Console.WriteLine("=================> Server was stoped");
            return true;
        }

        private TcpServerMachineTransition DefaultTransitions(MachineState state, StringBuilder input)
        {
            CommandInfo info = Parser.Parse(input.ToString().Split(' '));
            Boolean result = Validator.Validate(info, state >= MachineState.Initialized);
            if(!result)
                return new TcpServerMachineTransition(false, false, null);
            if (info.Command == CommandType.Quit)
            {
                if (state == MachineState.Started)
                    return new TcpServerMachineTransition(true, true,
                        new List<Tuple<TcpServerState, Object[]>>()
                        {
                            new Tuple<TcpServerState, Object[]>(new TcpServerState(MachineState.Stopped), null)
                        });
                return new TcpServerMachineTransition(true, true, null);
            }
            if (info.Command == CommandType.Start && state != MachineState.Started)
            {
                return new TcpServerMachineTransition(true, false,
                        new List<Tuple<TcpServerState, Object[]>>()
                        {
                            new Tuple<TcpServerState, Object[]>(new TcpServerState(MachineState.Started), 
                                                                new Object[]{info.IpAddress, info.Port, info.SettingsFile, info.ScriptFile})
                        });
            }
            if (info.Command == CommandType.Stop && state == MachineState.Started)
            {
                return new TcpServerMachineTransition(true, false,
                        new List<Tuple<TcpServerState, Object[]>>()
                        {
                            new Tuple<TcpServerState, Object[]>(new TcpServerState(MachineState.Stopped), null)
                        });
            }
            if (info.Command == CommandType.Restart && state == MachineState.Started)
            {
                return new TcpServerMachineTransition(true, false,
                        new List<Tuple<TcpServerState, Object[]>>()
                        {
                             new Tuple<TcpServerState, Object[]>(new TcpServerState(MachineState.Stopped), null),
                            new Tuple<TcpServerState, Object[]>(new TcpServerState(MachineState.Started), 
                                                                new Object[]{info.IpAddress, info.Port, info.SettingsFile, info.ScriptFile})
                        });
            }
            if (info.Command == CommandType.Help)
                return new TcpServerMachineTransition(true, false, null);
            return new TcpServerMachineTransition(false, false, null);
        }

        private ILog _logger;
        private ITcpServer _server;
        private MachineState _currentState = MachineState.Initial;
        private Func<MachineState, StringBuilder, TcpServerMachineTransition> _transitionFunc;
        private readonly IList<MachineState> _states = new List<MachineState>();
    }
}
