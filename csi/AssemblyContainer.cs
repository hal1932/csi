using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace csi
{
    public class AssemblyContainer : MarshalByRefObject, IDisposable
    {
        public bool Success { get { return (_assembly != null); } }

        public List<string> SourceFileList { get; private set; }
        public List<string> ReferenceList { get; private set; }


        private string _assemblyDir;
        private Assembly _assembly;


        public AssemblyContainer(
            string assemblyPath,
            Assembly assembly,
            List<string> sourceFileList, List<string> referenceList)
        {
            _assemblyDir = Path.GetDirectoryName(assemblyPath);
            _assembly = assembly;
            SourceFileList = sourceFileList;
            ReferenceList = referenceList;
        }


        public override object InitializeLifetimeService() { return null; }


        #region IDisposable
        private bool _disposed;
        ~AssemblyContainer()
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


        public void ExecuteMain(string[] args, bool removeCopiedDlls = false)
        {
            var main = _assembly.EntryPoint;
            if (main == null)
            {
                throw new InvalidOperationException("エントリポイントがありません");
            }

            var copiedReferences = new List<string>();
            foreach (var reference in ReferenceList)
            {
                try
                {
                    var path = Path.Combine(_assemblyDir, Path.GetFileName(reference));
                    File.Copy(reference, path, true);
                    copiedReferences.Add(path);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
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
