using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace csi
{
    class ScriptExecutable
    {
        public AssemblyContainer CompileResult { get; private set; }

        private bool _useOtherAppDomain;
        private AppDomain _appDomain;


        public ScriptExecutable(bool useOtherAppDomain = false)
        {
            _useOtherAppDomain = useOtherAppDomain;
        }


        public bool Build(
            string sourceFile,
            List<string> defineList, int warningLevel,
            List<string> requireDirList, List<string> libraryList)
        {
            if (CompileResult != null)
            {
                CompileResult.Dispose();
            }

            Compiler compiler;
            if (_useOtherAppDomain)
            {
                if (_appDomain != null)
                {
                    AppDomain.Unload(_appDomain);
                }

                _appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());

                var currentAssemblyName = Assembly.GetExecutingAssembly().FullName;
                compiler = (Compiler)_appDomain.CreateInstanceAndUnwrap(
                    currentAssemblyName, typeof(Compiler).FullName);
            }
            else
            {
                compiler = new Compiler();
            }

            CompileResult = compiler.Compile(
                sourceFile,
                defineList, warningLevel,
                requireDirList, libraryList);

            return (CompileResult != null);
        }


        public void ExecuteEntryPoint(string[] args)
        {
            if (CompileResult == null)
            {
                throw new InvalidOperationException("Compile is failed");
            }
            CompileResult.ExecuteMain(args);
        }
    }
}
