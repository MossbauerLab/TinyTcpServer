using System;
using System.IO;
using System.Threading;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CSharp;

namespace MossbauerLab.TinyTcpServer.Core.Server
{
    public class FlexibleTcpServer : TcpServer
    {
        public FlexibleTcpServer(String scriptFile)
        {
            if(String.IsNullOrEmpty(scriptFile))
                throw new ArgumentNullException("scriptFile");
            if(!File.Exists(Path.GetFullPath(scriptFile)))
                throw new ApplicationException("script");
            // todo: umv : maybe check what is inside script (some functions presence)
            _scriptFile = scriptFile;
            // compiler settings ...
            _parameters.GenerateExecutable = false;
            _parameters.GenerateInMemory = true;
        }

        public override Boolean Start()
        {
            Boolean result = base.Start();
            if (result)
                Execute();
            return result;
        }

        public override void Stop(Boolean clearHandlers)
        {
            //Terminate();
            base.Stop(clearHandlers);
        }

        private void Execute()
        {
            String scriptCode = File.ReadAllText(_scriptFile);
            _cancellationTokenSource = new CancellationTokenSource();
            CompilerResults results = _provider.CompileAssemblyFromSource(_parameters, new String[] {scriptCode});
            if (!results.Errors.HasErrors)
            {
                // executing ...
                Console.WriteLine("There is no errors!");
            }
            /*_state = _state == null
                   ? CSharpScript.RunAsync(scriptCode, GetScriptOptions(), null, null, _cancellationTokenSource.Token).Result
                   : _state.ContinueWithAsync(scriptCode, GetScriptOptions(), null, _cancellationTokenSource.Token).Result;
            SetVariables();*/

        }

/*        private void Terminate()
        {
            if (_state != null && _cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();
        }*/

/*        private ScriptOptions GetScriptOptions()
        {
            ScriptOptions options = ScriptOptions.Default;
            // add all references
            // assemblies
            options.AddReferences(Assembly.GetAssembly(typeof(TcpServer)));
            // namespaces
            options.AddImports("MossbauerLab.TinyTcpServer.Core.Client");
            options.AddImports("MossbauerLab.TinyTcpServer.Core.Handlers");
            options.AddImports("MossbauerLab.TinyTcpServer.Core.Handlers.Utils");
            return options;
        }*/

        private void SetVariables()
        {
            /*if (_state != null)
            {
                _state.Variables["_tcpServer"] = this;
            }*/
        }

        //private ScriptState<Object> _state;
        private CSharpCodeProvider _provider = new CSharpCodeProvider(new Dictionary<String, String>()
        {
            {"CompilerVersion", "v4.0"}
        });

        private CompilerParameters _parameters = new CompilerParameters(new []
        {
            Assembly.GetAssembly(typeof(TcpServer)).Location
        });


        private readonly String _scriptFile;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    }
}
