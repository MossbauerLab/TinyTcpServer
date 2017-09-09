using System;
using System.IO;
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
            Execute();
            return result;
        }

        private void Execute()
        {
            String scriptCode = File.ReadAllText(_scriptFile);
            _state = _state == null ? CSharpScript.RunAsync(scriptCode).Result : _state.ContinueWithAsync(scriptCode).Result;
        }

        private ScriptState<Object> _state;
        private readonly String _scriptFile;
    }
}
