using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csi
{
    class Preprocessor
    {
        public class Result
        {
            public List<string> SourceFileList { get; set; }
            public List<string> ReferenceList { get; set; }

            public Result()
            {
                SourceFileList = new List<string>();
                ReferenceList = new List<string>();
            }
        }


        public Result Preprocess(
            string sourceFile,
            List<string> requireDirList, List<string> refDirList)
        {
            var result = new Result();
            result.SourceFileList.Add(sourceFile);

            string[] sourceLineArray;
            using (var reader = new StreamReader(sourceFile))
            {
                sourceLineArray = reader.ReadToEnd().Split('\n')
                    .Select(line => line.Trim())
                    .ToArray();
            }

            var rootDir = Path.GetDirectoryName(sourceFile);

            // require
            if(requireDirList != null && requireDirList.Count> 0)
            { 
                foreach (var line in sourceLineArray)
                {
                    if (line.StartsWith("///require:"))
                    {
                        var require = line.Split(':')[1].Trim();
                        require = ResolvePath(require, rootDir, requireDirList);
                        if (require != null)
                        {
                            result.SourceFileList.Add(require);
                        }
                    }
                }
            }

            // addref
            if(refDirList != null && refDirList.Count > 0)
            {
                foreach (var line in sourceLineArray)
                {
                    if (line.StartsWith("///addref:"))
                    {
                        var addref = line.Split(':')[1].Trim();
                        addref = ResolvePath(addref, rootDir, refDirList);
                        if (addref != null)
                        {
                            result.ReferenceList.Add(addref);
                        }
                    }
                }
            }

            return result;
        }


        private string ResolvePath(string filename, string rootDir, List<string> refferredDirList)
        {
            var path = Path.Combine(rootDir, filename);
            if (File.Exists(path))
            {
                return Path.GetFullPath(path);
            }

            foreach (var dir in refferredDirList)
            {
                path = Path.Combine(dir, filename);
                if(File.Exists(path))
                {
                    return Path.GetFullPath(path);
                }
            }

            return null;
        }
    }
}
