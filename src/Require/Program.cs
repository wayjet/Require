using ICSharpCode.SharpZipLib.Zip;
using NDesk.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Yahoo.Yui.Compressor;

namespace Require
{
    class Program
    {
        static void Main(string[] args)
        {
            // -cmd=list -repository="D:\Wayjet\libs\js.core" -filter=jquery
            // -cmd=detail -repository="D:\Wayjet\libs\js.core" -require="jquery,jquery.ui.core"
            // -cmd=zip -repository="D:\Wayjet\libs\js.core" -require="jquery.ui.core" -output="D:\wayjet\test.zip"
            // -cmd=compress 未实现

            bool help = false;
            string command = null;
            string repositoryPath = null;
            string filter = null;
            string require = null;
            string ignore = null;
            string output = null;


            var p = new OptionSet(){
                {"cmd|command=","执行的操作",v=> command=v  },
                {"repository=", "源代码仓库的文件夹,该文件夹必须包含jspec文件", (v)=>repositoryPath=v  },
                {"filter=", "查看仓库中包含的包",(v)=>filter=v},
                {"require=","需要引用的包",v=>require=v},
                {"ignore=","需要引用的包",v=>ignore=v},
                {"output=", "zip文件输出的位置", v=>output =v},
                { "h|?|help",   v => help = v != null },
                //{ "v|verbose",  v => { ++verbose } },  
            };

            try
            {
                List<string> extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            if (help || args.Length==0)
            {
                ShowHelp(p);
                return; 
            }

            repositoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, repositoryPath);
            if (!Directory.Exists(repositoryPath))
            {
                Console.WriteLine("未找到目录 {0} ", repositoryPath);
                return;
            }

            var repository = LoadRepository(repositoryPath);
            switch (command.ToLower())
            {
                case "list":
                    {
                        List(repository, filter);
                        break;
                    }
                case "detail":
                    {
                        if (string.IsNullOrEmpty(require))
                        {
                            Console.WriteLine("未指定 require 参数", output);
                            return;
                        }
                        var requireOptions = BuildRequire(require);
                        foreach (var pair in requireOptions)
                        {
                            Console.WriteLine("以下是 {0} - {1} 所需要依赖的包", pair.Key, pair.Value);
                            var packages = repository.GetPackageDependencies(new Dictionary<string, Version>() { 
                                { pair.Key, pair.Value }
                            });
                            foreach (var package in packages)
                            {
                                Console.WriteLine("{0} - {1}", package.Name, package.Version);
                            }
                        }
                        break;
                    }
                case "zip":
                    {
                        if (string.IsNullOrEmpty(output))
                        {
                            Console.WriteLine("未指定 output 参数", output);
                            return;
                        }
                        if (string.IsNullOrEmpty(require))
                        {
                            Console.WriteLine("未指定 require 参数", output);
                            return;
                        }
                        var requireOptions = BuildRequire(require);
                        var packages = repository.GetPackageDependencies(requireOptions);
                        Zip(output, packages);
                        Console.WriteLine("已经打包到 {0}", output);
                        break;
                    }
                case "compress":
                    {
                        throw new NotSupportedException();
                        break;
                    }
                default:
                    {
                        ShowHelp(p);
                        break;
                    }
            }
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: greet [OPTIONS]+ message");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static PackageRepository LoadRepository(string path)
        {
            List<Package> packages = new List<Package>();
            foreach (var filePath in Directory.GetFiles(path, "*.jspec", SearchOption.AllDirectories))
            {
                string json = File.ReadAllText(filePath);

                List<PackageSpec> specs = null;
                try
                {
                    specs = JsonConvert.DeserializeObject<List<PackageSpec>>(json);
                }
                catch(Exception ex)
                {
                    throw new Exception(string.Format("{0}格式错误", filePath), ex);
                }
                

                foreach (var spec in specs)
                {
                    spec.FilePath = filePath;
                    var package = spec.CreatePackage();
                    packages.Add(package);
                }
            }

            PackageRepository repository = new PackageRepository();
            repository.AddPackages(packages);
            return repository;
        }

        static void List(PackageRepository repository, string filter)
        {
            foreach (var pkg in repository.GetPackages(filter))
            {
                Console.WriteLine("{0} - {1}", pkg.Name, pkg.Version);
            }
        }

        static void Zip(string output, IEnumerable<Package> packages)
        {
            using (FileStream fileStream = new FileStream(output, FileMode.Create))
            using (ZipOutputStream zipStream = new ZipOutputStream(fileStream))
            {
                var buffer = new byte[4096];

                foreach (var pkg in packages)
                {
                    var sourceBaseDirUri = new Uri(pkg.BaseDir);
                    string targetDir = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", pkg.Name, pkg.Version);

                    foreach (var file in pkg.Files)
                    {
                        var relativePath = sourceBaseDirUri.MakeRelativeUri(new Uri(file)).ToString();

                        var entry = new ZipEntry(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", targetDir, relativePath));
                        zipStream.PutNextEntry(entry);

                        using (var fileReader = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            int read = 0;
                            while ((read = fileReader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                zipStream.Write(buffer, 0, read);
                            }
                        }
                    }

                    var spec = new PackageSpec()
                    {
                        Name = pkg.Name,
                        Version = pkg.Version.ToString()
                    };
                    spec.Files.Add(new PackageSpec.FileSet()
                    {
                        Include = "**/*.*",
                        Exclude = "**/*.jspec"
                    });
                    foreach (var depend in pkg.Dependencies)
                    {
                        string dependString = string.Format(CultureInfo.InvariantCulture, "{0} ~ {1}", depend.MinVersion, depend.MaxVersion);
                        spec.Dependencies.Add(depend.Name, dependString);
                    }

                    var specEntry = new ZipEntry(string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}/{1}-{2}.jspec",
                        targetDir,
                        spec.Name,
                        spec.Version));
                    zipStream.PutNextEntry(specEntry);
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(new PackageSpec[] { spec }, Formatting.Indented);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    zipStream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        static void Compress(string output, IEnumerable<Package> packages)
        {
            StringBuilder source = new StringBuilder();
            foreach (var pkg in packages)
            {
                foreach (var file in pkg.Files)
                {
                    string src = File.ReadAllText(file);
                    source.Append(src);
                }
            }

            JavaScriptCompressor c = new JavaScriptCompressor();
            c.Encoding = Encoding.UTF8;
            string result = c.Compress(source.ToString());

            File.WriteAllText(output, result);
        }

        static IDictionary<string, Version> BuildRequire(string require)
        {
            var result = new Dictionary<string, Version>();

            var array = require.Split(new char[] { ',', ';' },  StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in array)
            {
                var array2 = item.Split(new char[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string name = array2[0];
                Version version = array2.Length > 1 ? Version.Parse(array2[1]) : null;
                result.Add(name, version);
            }
            return result;
        }
    }

    


    

    
    /*
      string output = @"D:\Wayjet\libs\js.core\src\JsPack\Unpack";
            string nupkg = @"D:\Wayjet\libs\js.core\src\JsPack\Wayjet.Core.1.0.56.37697.nupkg";
            using(FileStream fileStream = new FileStream(nupkg,FileMode.Open))
            using(ZipInputStream zipStream = new ZipInputStream(fileStream))
            {
                var buffer = new byte[4096];

                ZipEntry entry = null;
                while ((entry = zipStream.GetNextEntry()) != null) {
                    string fileName = Path.Combine(output, entry.Name);
                    string dirName = Path.GetDirectoryName(fileName);

                    Directory.CreateDirectory(dirName);

                    Console.WriteLine(entry.Name);
                    using (FileStream streamWriter = File.Create(fileName)) 
                    {
                        int readed = 0;
                        while ((readed = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            streamWriter.Write(buffer, 0, readed);
                        }
                    }

                    
                }
            }
     */


//    public class CodeFetcher : IFetcher
//    {
//        public FileDescriptor GetDescriptor(Stream stream)
//        {
//            var buffer = new byte[4096];
//            Regex regex = new Regex(@"
//@require\s+""(?<_ID_>.*?)""\s*
//(  (?<SPEC>\d+(\.\d+)*)  
//|  (
//       (?<MINOP>\[|\()\s*
//       (?<MIN>\d+(\.\d+)*\s*)?
//       ,\s*
//       (?<MAX>\d+(\.\d+)*\s*)?
//       (?<MAXOP>\]|\))
//   )
//)");

//            using (TextReader reader = new StreamReader(stream,true)) {
//                string content = reader.ReadToEnd();

//                //提取第一个注释块
                
//            }
//            //using(StringReader reader = 
//            //throw new NotImplementedException();
//        }
//    }
}
