using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csi
{
    class Program
    {
        class Options
        {
            [CommandLineArg("define", Type = CommandLineArgType.StringList, Description = "マクロ定義")]
            public List<string> DefineList { get; set; }

            [CommandLineArg("warn", Type = CommandLineArgType.Integer, DefaultValue = 4, Description = "警告レベル")]
            public int WarningLevel { get; set; }

            [CommandLineArg("require", Type = CommandLineArgType.StringList, Description = "requireディレクトリ")]
            public List<string> RequireDirList { get; set; }

            [CommandLineArg("lib", Type = CommandLineArgType.StringList, Description = "DLLディレクトリ")]
            public List<string> LibraryList { get; set; }

            [CommandLineArg("watch", Type = CommandLineArgType.Boolean, Description = "ファイル監視して自動ビルド")]
            public bool WatchFiles { get; set; }
        }


        static void Main(string[] args)
        {
            var parser = new CommandLineArgsParser<Options>();
            if (!parser.Parse()) return;

            var options = parser.Options;
            var values = parser.Values;

            var sourceFile = Path.GetFullPath(values[0]);
            var scriptArgs = values.Skip(1).ToArray();

            if(!options.WatchFiles)
            {
                var script = new ScriptExecutable();
                BuildAndExecute(
                    script,
                    sourceFile, scriptArgs,
                    options.DefineList, options.WarningLevel,
                    options.RequireDirList, options.LibraryList);
            }
            else
            {
                var script = new ScriptExecutable(true);
                BuildAndExecute(
                    script,
                    sourceFile, scriptArgs,
                    options.DefineList, options.WarningLevel,
                    options.RequireDirList, options.LibraryList);

                var targetFileList = script.CompileResult.SourceFileList;
                targetFileList.AddRange(script.CompileResult.ReferenceList);

                var watcher = new FileEditWatcher();
                watcher.Start(targetFileList, (updatedFile) =>
                {
                    Console.WriteLine(string.Format("{0} is updated", updatedFile));
                    BuildAndExecute(
                        script,
                        sourceFile, scriptArgs,
                        options.DefineList, options.WarningLevel,
                        options.RequireDirList, options.LibraryList);
                });

                while (true)
                {
                    Thread.Sleep(int.MaxValue);
                }
            }
        }


        private static void BuildAndExecute(
            ScriptExecutable script,
            string sourceFile, string[] scriptArgs,
            List<string> defineList, int warningLevel,
            List<string> requireDirList, List<string> libraryList)
        {
            var success = script.Build(
                sourceFile,
                defineList, warningLevel,
                requireDirList, libraryList);
            if (success)
            {
                script.ExecuteEntryPoint(scriptArgs);
            }
        }
    }
}
