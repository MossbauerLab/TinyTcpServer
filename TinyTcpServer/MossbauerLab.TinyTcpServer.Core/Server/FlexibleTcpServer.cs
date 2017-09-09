using System;
using System.IO;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

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
            Terminate();
            base.Stop(clearHandlers);
        }

        private void Execute()
        {
            String scriptCode = File.ReadAllText(_scriptFile);
            _cancellationTokenSource = new CancellationTokenSource();
            _state = _state == null
                   ? CSharpScript.RunAsync(scriptCode, null, null, null, _cancellationTokenSource.Token).Result
                   : _state.ContinueWithAsync(scriptCode, null, null, _cancellationTokenSource.Token).Result;
        }

        private void Terminate()
        {
            if (_state != null && _cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();
        }

        private ScriptState<Object> _state;
        private readonly String _scriptFile;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    }
}
