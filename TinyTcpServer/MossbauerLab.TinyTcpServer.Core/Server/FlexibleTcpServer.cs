using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Microsoft.CSharp;
using MossbauerLab.TinyTcpServer.Core.Scripting;

namespace MossbauerLab.TinyTcpServer.Core.Server
{
    public class FlexibleTcpServer : TcpServer
    {
        public FlexibleTcpServer(String scriptFile, String ipAddress, UInt16 port, CompilerOptions compilerOptions = null,
                                 ILog logger = null, Boolean debug = false, TcpServerConfig config = null)
            :base(ipAddress, port, logger, debug, config)
        {
            if(String.IsNullOrEmpty(scriptFile))
                throw new ArgumentNullException("scriptFile");
            if(!File.Exists(Path.GetFullPath(scriptFile)))
                throw new ApplicationException("script");
            // todo: umv : maybe check what is inside script (some functions presence)
            _scriptFile = scriptFile;
            if (compilerOptions == null)
                _compilerOptions = new CompilerOptions();

        }

        public override Boolean Start()
        {
            Boolean result = base.Start();
            if (result)
                Execute();
            return result;
        }

        private void Execute()
        {
            String scriptCode = File.ReadAllText(_scriptFile);
            CompilerResults results = _compilerOptions.Provider.CompileAssemblyFromSource(_compilerOptions.Parameters, new [] {scriptCode});
            if (!results.Errors.HasErrors)
            {
                // executing ...
                Type mainType = results.CompiledAssembly.GetType(_compilerOptions.ScriptEntryType);
                MethodInfo methodInfo = mainType.GetMethod("Init");
                if(methodInfo == null)
                    throw new ApplicationException(String.Format("Script do not contain Init method in {0}", _compilerOptions.ScriptEntryType));
                Object instance = Activator.CreateInstance(mainType);
                methodInfo.Invoke(instance, new Object[] {this});
            }
        }

        private readonly String _scriptFile;
        private readonly CompilerOptions _compilerOptions;
    }
}
