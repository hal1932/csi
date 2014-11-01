using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csi
{
    class Program
    {
        public class Options
        {
            [CommandLineArg("define", Type = CommandLineArgType.StringList, Description = "マクロ定義")]
            public List<string> DefineList { get; set; }

            [CommandLineArg("warn", Type = CommandLineArgType.Integer, DefaultValue = 4, Description = "警告レベル")]
            public int WarningLevel { get; set; }

            [CommandLineArg("require", Type = CommandLineArgType.StringList, Description = "requireディレクトリ")]
            public List<string> RequireDirList { get; set; }

            [CommandLineArg("lib", Type = CommandLineArgType.StringList, Description = "DLLディレクトリ")]
            public List<string> LibraryList { get; set; }
        }


        static void Main(string[] args)
        {
            var parser = new CommandLineArgsParser<Options>();
            if (!parser.Parse()) return;

            var options = parser.Options;
            var values = parser.Values;

            var sourceFile = Path.GetFullPath(values[0]);

            var compiler = new Compiler();
            var result = compiler.Build(
                sourceFile,
                options.DefineList, options.WarningLevel,
                options.RequireDirList, options.LibraryList);

            if (result != null)
            {
                var scriptArgs = values.Skip(1).ToArray();
                result.ExecuteMain(scriptArgs);
            }

            Console.ReadKey();
        }
    }
}
