using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Require
{
    public class PackageRepository
    {
        private Dictionary<string, Dictionary<Version, Package>> allPackages = new Dictionary<string, Dictionary<Version, Package>>();

        public void AddPackages(IEnumerable<Package> packages)
        {
            foreach (var package in packages)
            {
                AddPackage(package);
            }
        }

        public IEnumerable<Package> GetPackages(string keyword)
        {
            foreach (var pair1 in this.allPackages)
            {
                foreach (var pair2 in pair1.Value)
                {
                    var pkg = pair2.Value;
                    if (pkg.Name.Contains(keyword)) yield return pkg;
                }
            }
        }

        public void AddPackage(Package package)
        {
            Dictionary<Version, Package> product = null;
            if (!allPackages.TryGetValue(package.Name, out product))
            {
                product = new Dictionary<Version, Package>();
                allPackages.Add(package.Name, product);
            }

            if (product.ContainsKey(package.Version))
            {
                throw new InvalidOperationException(string.Format("已经存在包 {0}-{1}", package.Name, package.Version));
            }
            product.Add(package.Version, package);
        }

        /// <summary>
        /// 查找所有包的依赖
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public IEnumerable<Package> GetPackageDependencies(IDictionary<string, Version> products)
        {
            // 获取所有需要的产品的版本
            List<Package> packages = new List<Package>();
            foreach (var pair in products)
            {
                string name = pair.Key;
                Version version = pair.Value;
                Dictionary<Version, Package> product = null;
                if (!allPackages.TryGetValue(name, out product))
                {
                    throw new Exception(string.Format("没有找到 {0}", name));
                }

                Package package = null;
                if (version == null) version = product.Values.Max(m => m.Version);

                if (!product.TryGetValue(version, out package))
                {
                    throw new Exception(string.Format("没有找到 {0}-{1}", name, version));
                }
                packages.Add(package);
            }


            // 提取每个包所匹配的版本
            var stack = new Stack<PackageDependencyTreeNode>();
            var root = new PackageDependencyTreeNode();

            foreach (var package in packages)
            {
                var node = root.Children.Add(package);
                stack.Push(node);
            }

            while (stack.Count > 0)
            {
                var node = stack.Pop();

                foreach (var depend in node.Data.Dependencies)
                {
                    var packageAvailable = this.GetPackages(depend.Name, depend.MinVersion, depend.MaxVersion).ToList();

                    if (packageAvailable.Count == 0)
                    {
                        throw new Exception(string.Format("找不到 {0} 版本在{1} ~ {2}", depend.Name, depend.MinVersion, depend.MaxVersion));
                    }
                    foreach (var pkg in packageAvailable)
                    {
                        var subNode = node.Children.Add(pkg);
                        stack.Push(subNode);
                    }
                }
            }


            Dictionary<string, Package> requires = new Dictionary<string, Package>();

            // 提取无任何依赖的包作为检查的包
            var checkPackages = (from item in root.Find((n) => { return n.Data.Dependencies.Count == 0; }, false)
                                 group item by item.Data.Name into g
                                 select g
                                 ).ToDictionary(
                                    m => m.Key,
                                    m => m.ToList()
                                 );

            while (checkPackages.Count > 0)
            {
                List<Package> bestPackages = new List<Package>();

                // 检查最适合的包版本，既是找出可以供所有包所使用的最高版本
                foreach (var pair in checkPackages)
                {
                    string name = pair.Key;
                    var nodes = pair.Value.Select(m => m.Parent).Distinct().ToList(); //所有的依赖包
                    var pkgs = pair.Value.Select(m => m.Data).Distinct().OrderByDescending(m => m.Version); //所有的被依赖包的版本

                    Package bestPackage = null;
                    foreach (var pkg in pkgs)
                    {
                        bool fail = false;
                        foreach (var node in nodes)
                        {
                            if (node.Children.FirstOrDefault(m => m.Data == pkg) == null)
                            {
                                fail = true;
                                break;
                            }
                        }

                        if (!fail)
                        {
                            bestPackage = pkg;
                            break;
                        }
                    }

                    if (bestPackage == null)
                    {
                        // TODO : 更详细的信息
                        throw new Exception(string.Format("引用 {0} 出现冲突", name));
                    }
                    else
                    {
                        bestPackages.Add(bestPackage);
                    }
                }

                foreach (var pkg in bestPackages)
                {
                    requires.Add(pkg.Name, pkg);
                }


                //进行下一个节点的检查
                var checkNodes = root.Find((n) =>
                {
                    foreach (var d in n.Data.Dependencies)
                    {
                        if (bestPackages.FirstOrDefault(m => m.Name == d.Name) != null) return true;
                    }
                    return false;
                }, false).Where(n => !requires.ContainsKey(n.Data.Name));

                checkPackages = (from item in checkNodes
                                 group item by item.Data.Name into g
                                 select g
                                ).ToDictionary(
                                    m => m.Key,
                                    m => m.ToList()
                                );
            }
            return requires.Values;
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        private IEnumerable<Package> GetPackages(string name, DependencyVersion min, DependencyVersion max)
        {
            Dictionary<Version, Package> product = null;
            if (!allPackages.TryGetValue(name, out product))
            {
                throw new Exception(string.Format("没有找到 {0}", name));
            }

            List<Package> result = new List<Package>();
            foreach (var pair in product)
            {
                Version v = pair.Key;
                Package p = pair.Value;

                if (VersionGreaterThanOrEqual(v, min) && VersionLessThanOrEqual(v, max))
                {
                    result.Add(p);
                }
            }
            return result;
        }

        private bool VersionGreaterThanOrEqual(Version version, DependencyVersion test)
        {
            if(test==null) return true;

            if (test.Major != null)
            {
                int compare = version.Major.CompareTo(test.Major.Value);
                if (compare < 0)
                {
                    return false;
                }
                else if (compare > 0)
                {
                    return true;
                }   
            }

            if (test.Minor != null)
            {
                int compare = version.Minor.CompareTo(test.Minor.Value);
                if (compare < 0)
                {
                    return false;
                }
                else if (compare > 0)
                {
                    return true;
                }
            }
            
            if(test.Build!=null)
            {
                int compare = version.Build.CompareTo(test.Build.Value);
                if (compare < 0)
                {
                    return false;
                }
                else if (compare > 0)
                {
                    return true;
                }
            }

            if (test.Revision != null)
            {
                int compare = version.Revision.CompareTo(test.Revision.Value);
                if (compare < 0)
                {
                    return false;
                }
                else if (compare > 0)
                {
                    return true;
                }
            }
            return true;
        }

        private bool VersionLessThanOrEqual(Version version, DependencyVersion test)
        {
            if (test == null) return true;

            if (test.Major != null)
            {
                int compare = version.Major.CompareTo(test.Major.Value);
                if (compare > 0)
                {
                    return false;
                }
                else if (compare < 0)
                {
                    return true;
                }
            }

            if (test.Minor != null)
            {
                int compare = version.Minor.CompareTo(test.Minor.Value);
                if (compare > 0)
                {
                    return false;
                }
                else if (compare < 0)
                {
                    return true;
                }
            }

            if (test.Build != null)
            {
                int compare = version.Build.CompareTo(test.Build.Value);
                if (compare > 0)
                {
                    return false;
                }
                else if (compare < 0)
                {
                    return true;
                }
            }

            if (test.Revision != null)
            {
                int compare = version.Revision.CompareTo(test.Revision.Value);
                if (compare > 0)
                {
                    return false;
                }
                else if (compare < 0)
                {
                    return true;
                }
            }

            return true;
        }
    }
}
