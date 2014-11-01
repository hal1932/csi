using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csi
{
    class FileEditWatcher
    {
        private List<FileSystemWatcher> _watherList = new List<FileSystemWatcher>();
        private Dictionary<string, DateTime> _fileEditHistory = new Dictionary<string, DateTime>();


        public void Start(List<string> filenameList, Action<string> onEditFile)
        {
            foreach (var filename in filenameList)
            {
                _fileEditHistory[filename] = new FileInfo(filename).LastWriteTime;

                var watcher = new FileSystemWatcher();
                watcher.Path = Path.GetDirectoryName(filename);
                watcher.Filter = Path.GetFileName(filename);
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = false;
                watcher.Changed += (sender, e) =>
                {
                    var fileInfo = new FileInfo(e.FullPath);
                    var prevLastWriteTime = _fileEditHistory[fileInfo.FullName];
                    if (fileInfo.LastWriteTime > prevLastWriteTime)
                    {
                        onEditFile(fileInfo.FullName);
                        _fileEditHistory[fileInfo.FullName] = fileInfo.LastWriteTime;
                    }
                };
                _watherList.Add(watcher);

                watcher.EnableRaisingEvents = true;
            }
        }


        public void Stop()
        {
            foreach (var watcher in _watherList)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _watherList.Clear();
        }
    }
}
