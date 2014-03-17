using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Require
{
    public class PackageSpec
    {
        /// <summary>
        /// 占位符替换器
        /// </summary>
        private static Regex _HolderRegex = new Regex(@"\{(version|name)\}", RegexOptions.IgnoreCase);

        [JsonIgnore]
        public string FilePath { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string BaseDir { get; set; }

        public string Description { get; set; }

        public List<FileSet> Files { get; private set; }

        

        public Dictionary<string,string> Dependencies { get; private set; }

        public PackageSpec()
        {
            this.Files = new List<FileSet>();
            this.Dependencies = new Dictionary<string, string>();
        }

        public Package CreatePackage()
        {
            var baseDir = Path.Combine(Path.GetDirectoryName(this.FilePath), WithPlaceHolder(this.BaseDir)) + "/";

            Package package = new Package(this.Name, System.Version.Parse(this.Version), baseDir, this);

            // 提取依赖
            foreach (var pair in this.Dependencies)
            {
                PackageDependency depend = new PackageDependency()
                {
                    Name = pair.Key
                };

                // 用占位符替换
                string ver = WithPlaceHolder(pair.Value).Trim();

                // 分解依赖的版本号
                if(ver.StartsWith("~", StringComparison.InvariantCulture))
                {
                    depend.MaxVersion = DependencyVersion.Parse(ver.Substring(1).Trim());
                }
                else if(ver.EndsWith("~", StringComparison.InvariantCulture))
                {
                    depend.MinVersion = DependencyVersion.Parse(ver.Substring(0,ver.Length-1).Trim());
                }
                else if(ver.IndexOf('~')>0)
                {
                    var arr = ver.Split('~');
                    depend.MinVersion = DependencyVersion.Parse(arr[0].Trim());
                    depend.MaxVersion = DependencyVersion.Parse(arr[1].Trim());
                }
                else
                {
                    depend.MinVersion = depend.MaxVersion = DependencyVersion.Parse(ver);
                }
                package.Dependencies.Add(depend);
            }

            // 提取包里面所包含的文件
            List<string> files = new List<string>();
            
            foreach (var fileset in this.Files)
            {
                var filesFound = GetFiles(baseDir, fileset);
                files.AddRange(filesFound);
            }
            package.Files.AddRange(files.Distinct());

            return package;
        }

        private string WithPlaceHolder(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            return _HolderRegex.Replace(input, (mt) =>
            {
                if (string.Equals(mt.Value, "{name}"))
                {
                    return this.Name;
                }
                else if (string.Equals(mt.Value, "{version}"))
                {
                    return this.Version;
                }
                else
                {
                    return string.Empty;
                }
            });
        }

        private IEnumerable<string> GetFiles(string baseDir, FileSet fileSet)
        {
            var separator = new char[] { ';', '\r', '\n' };

            var includes = WithPlaceHolder(fileSet.Include).Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var excludes = WithPlaceHolder(fileSet.Exclude).Split(separator, StringSplitOptions.RemoveEmptyEntries);

            List<string> filesInclude = new List<string>();
            List<string> filesExclude = new List<string>();

            foreach (var filter in includes)
            {
                filesInclude.AddRange(FilterFiles(baseDir, filter));
            }
            foreach (var filter in excludes)
            {
                filesExclude.AddRange(FilterFiles(baseDir, filter));
            }
            return filesInclude.Except(filesExclude);
        }

        private IEnumerable<string> FilterFiles(string baseDir, string filter)
        {
            if (filter.EndsWith("/") || filter.EndsWith("\\")) filter += "*.*";

            var separator = new char[] { '\\', '/'};
            var dirs = new List<DirectoryInfo>(); 
            dirs.Add(new DirectoryInfo(baseDir));

            //过滤子目录
            var paths = filter.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length > 1)
            {
                for (int i = 0; i < paths.Length - 1; ++i)
                {
                    var path = paths[i];

                    var subDirs = new List<DirectoryInfo>();

                    if (path == ".")
                    {
                        continue;
                    }
                    else if(path=="..")
                    {
                        foreach (var dir in dirs)
                        {
                            subDirs.Add(dir.Parent);
                        }
                        dirs = subDirs;
                    }
                    else if (path == "**")
                    {
                        foreach (var dir in dirs)
                        {
                            subDirs.AddRange(dir.GetDirectories("*", SearchOption.AllDirectories));
                        }
                        dirs.AddRange(subDirs);
                    }
                    else
                    {
                        foreach (var dir in dirs)
                        {
                            subDirs.AddRange(dir.GetDirectories(path, SearchOption.TopDirectoryOnly));
                        }
                        dirs = subDirs;
                    }
                }
            }
            

            var files = new List<string>();
            var fileFilter = paths[paths.Length - 1];
            if (fileFilter == "**")
            {
                foreach (var dir in dirs)
                {
                    files.AddRange(dir.GetFiles("*", SearchOption.AllDirectories).Select(m=>m.FullName));
                }
            }
            else
            {
                foreach (var dir in dirs)
                {
                    files.AddRange(dir.GetFiles(fileFilter, SearchOption.TopDirectoryOnly).Select(m=>m.FullName));
                }
            }

            return files;


            // dir1/dir*dir/**/dir3/*.fil.*
            // dir1/dir*dir/**/dir3/**

            //Directory.GetFiles()
            //var separator = new string[] { "**" };
            //var dirs = filter.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            //foreach (var dir in dirs)
            //{
            //    if (dir.Contains('*'))
            //    {
            //    }
            //    else
            //    {
            //    }
            //}
        }

        public class FileSet
        {
            private string _Include, _Exclude;

            public string Include 
            {
                get { return _Include ?? string.Empty; }
                set { _Include = value; }
            }

            public string Exclude 
            {
                get { return _Exclude ?? string.Empty; }
                set { _Exclude = value; }
            }
        }
    }

    
}
