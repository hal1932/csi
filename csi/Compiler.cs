using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace csi
{
    public class Compiler : MarshalByRefObject, IDisposable
    {
        public override object InitializeLifetimeService() { return null; }


        #region IDisposable
        private bool _disposed;
        ~Compiler()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
        }
        protected void Dispose(bool disposing)
        {
            if (_disposed) return;

            lock (this)
            {
                RemotingServices.Disconnect(this);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }
        #endregion


        public AssemblyContainer Compile(
            string filename,
            List<string> defineList, int warningLevel,
            List<string> requireDirList, List<string> libraryList)
        {
            var param = new CompilerParameters()
            {
                GenerateExecutable = true,
                GenerateInMemory = false,// DLL参照のため
                IncludeDebugInformation = false,
                WarningLevel = warningLevel,
            };

            var compilerOptions = new StringBuilder();
            {
                compilerOptions.Append("/optimize");
                if (defineList != null && defineList.Count > 0)
                {
                    foreach (var define in defineList)
                    {
                        compilerOptions.AppendFormat(" /define:{0}", define);
                    }
                }
            }
            param.CompilerOptions = compilerOptions.ToString();

            var references = new string[]
            {
                "System.dll",
                "mscorlib.dll",
                "System.Core.dll",
            };
            foreach (var reference in references)
            {
                param.ReferencedAssemblies.Add(reference);
            }

            var preprocessor = new Preprocessor();
            var target = preprocessor.Preprocess(filename, requireDirList, libraryList);
            var addedReferences = new List<string>();
            foreach (var reference in target.ReferenceList)
            {
                param.ReferencedAssemblies.Add(reference);
                addedReferences.Add(reference);
            }
            Debug.Assert(target.SourceFileList.Count > 0);

            var provider = new CSharpCodeProvider(
                new Dictionary<String, String> { { "CompilerVersion", "v3.5" } });
            var result = provider.CompileAssemblyFromFile(param, target.SourceFileList.ToArray());
            if (result.Errors.Count > 0)
            {
                foreach (CompilerError error in result.Errors)
                {
                    Console.Error.WriteLine(string.Format(
                        "[{0}] {1}, {2}:{3}",
                        error.ErrorNumber, error.ErrorText,
                        Path.GetFileName(error.FileName), error.Line));
                }
                return new AssemblyContainer(
                    null, null,
                    target.SourceFileList, addedReferences);
            }

            return new AssemblyContainer(
                param.OutputAssembly,
                result.CompiledAssembly,
                target.SourceFileList, addedReferences);
        }
    }
}
