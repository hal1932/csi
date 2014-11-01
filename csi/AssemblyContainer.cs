using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace csi
{
    class AssemblyContainer
    {
        private string _assemblyDir;
        private Assembly _assembly;
        private List<string> _requiredReferences;


        public AssemblyContainer(string assemblyPath, Assembly assembly, List<string> requiredReferences)
        {
            _assemblyDir = Path.GetDirectoryName(assemblyPath);
            _assembly = assembly;
            _requiredReferences = requiredReferences;
        }


        public void ExecuteMain(string[] args, bool removeCopiedDlls = false)
        {
            var main = _assembly.EntryPoint;
            if (main == null)
            {
                throw new InvalidOperationException("エントリポイントがありません");
            }

            var copiedReferences = new List<string>();
            foreach (var reference in _requiredReferences)
            {
                try
                {
                    var path = Path.Combine(_assemblyDir, Path.GetFileName(reference));
                    File.Copy(reference, path);
                    copiedReferences.Add(path);
                }
                catch (Exception)
                {
                    /// なにもしない
                }
            }

            main.Invoke(null, new object[] { args });

            if(removeCopiedDlls)
            {
                foreach (var reference in copiedReferences)
                {
                    if(File.Exists(reference))
                    {
                        File.Delete(reference);
                    }
                }
            }
        }
    }
}
