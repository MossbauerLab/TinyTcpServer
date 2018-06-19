using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using MossbauerLab.TinyTcpServer.Core.Scripting;

namespace MossbauerLab.TinyTcpServer.Console.Builders
{
    public static class CompilerOptionsBuilder
    {
        public static CompilerOptions Build(String compilerOptionsConfig)
        {
            CompilerOptions options = new CompilerOptions();
            if (String.IsNullOrEmpty(compilerOptionsConfig))
                throw new ArgumentNullException("compilerOptionsConfig");
            if (!File.Exists(compilerOptionsConfig))
                throw new ApplicationException("Config file does not exists");
            IList<String> content = File.ReadAllLines(compilerOptionsConfig).Select(line => line.Trim().ToLower())
                                                                            .Where(line => !String.IsNullOrEmpty(line))
                                                                            .Where(line => !line.StartsWith(CommentarySymbol))
                                                                            .ToList();
            String compilerVersion = GetConfigurationValue(content, CompilerVersionKey);
            String assembliesLine = GetConfigurationValue(content, AssembliesKey);
            String[] assemblies = assembliesLine.Split(',');
            String scriptEntryType = GetConfigurationValue(content, ScriptEntryTypeKey);

            options.Provider = new CSharpCodeProvider(new Dictionary<String, String>()
            {
                {"CompilerVersion", compilerVersion}
            });
            options.Parameters = new CompilerParameters(assemblies);
            options.Parameters.GenerateExecutable = false;
            options.Parameters.GenerateInMemory = true;
            options.ScriptEntryType = scriptEntryType;
            return options;
        }

        private static String GetConfigurationValue(IList<String> fileContent, String key)
        {
            try
            {
                String configLine = fileContent.FirstOrDefault(line => line.ToLower().StartsWith(key.ToLower()));
                if (configLine == null)
                    return null;
                Int32 index = configLine.IndexOf(KeyValueSeparator, StringComparison.InvariantCulture);
                if (index <= 0)
                    return null;
                String value = configLine.Substring(index + 1).Trim(' ','\t');
                return value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private const String KeyValueSeparator = "=";
        private const String CommentarySymbol = "#";

        private const String CompilerVersionKey = "CompilerVersion";
        private const String AssembliesKey = "Assemblies";
        private const String ScriptEntryTypeKey = "ScriptEntryType";
    }
}
