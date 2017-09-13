using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Microsoft.CSharp;

namespace MossbauerLab.TinyTcpServer.Core.Server
{
    public class FlexibleTcpServer : TcpServer
    {
        public FlexibleTcpServer(String scriptFile, String ipAddress, UInt16 port, ILog logger = null, Boolean debug = false, TcpServerConfig config = null)
            :base(ipAddress, port, logger, debug, config)
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

        private void Execute()
        {
            String scriptCode = File.ReadAllText(_scriptFile);
            CompilerResults results = _provider.CompileAssemblyFromSource(_parameters, new String[] {scriptCode});
            if (!results.Errors.HasErrors)
            {
                // executing ...
                Console.WriteLine("There is no errors!");
                Type mainType = results.CompiledAssembly.GetType(ScriptEntryType);
                MethodInfo methodInfo = mainType.GetMethod("Init");
                if(methodInfo == null)
                    throw new ApplicationException(String.Format("Script do not contain Init method in {0}", ScriptEntryType));
                Object instance = Activator.CreateInstance(mainType);
                methodInfo.Invoke(instance, new Object[] {this});
            }
        }

        private readonly CSharpCodeProvider _provider = new CSharpCodeProvider(new Dictionary<String, String>()
        {
            {"CompilerVersion", "v4.0"}
        });

        private readonly CompilerParameters _parameters = new CompilerParameters(new[]
        {
            Assembly.GetAssembly(typeof(TcpServer)).Location
        });

        public const String ScriptEntryType = "MossbauerLab.Flexibility.ServerScript";

        private readonly String _scriptFile;
    }
}
