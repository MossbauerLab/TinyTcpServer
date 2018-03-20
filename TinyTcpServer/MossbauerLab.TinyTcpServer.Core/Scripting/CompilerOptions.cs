using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CSharp;
using MossbauerLab.TinyTcpServer.Core.Server;

namespace MossbauerLab.TinyTcpServer.Core.Scripting
{
    public class CompilerOptions
    {
        public CompilerOptions()
        {
            Provider = new CSharpCodeProvider(new Dictionary<String, String>()
            {
                {"CompilerVersion", "v4.0"}
            });

            Parameters = new CompilerParameters(new[]
            {
                Assembly.GetAssembly(typeof(TcpServer)).Location
            });
            Parameters.GenerateExecutable = false;
            Parameters.GenerateInMemory = true;

            ScriptEntryType = DefaultScriptEntryType;
        }

        public CompilerOptions(CSharpCodeProvider provider, CompilerParameters parameters,
                               String scriptEntryType = DefaultScriptEntryType)
        {
            Provider = provider;
            Parameters = parameters;
            ScriptEntryType = scriptEntryType;
        }

        public String ScriptEntryType { get; set; }
        public CSharpCodeProvider Provider { get; set; }
        public CompilerParameters Parameters { get; set; }

        private const String DefaultScriptEntryType = "MossbauerLab.Flexibility.ServerScript";
    }
}
