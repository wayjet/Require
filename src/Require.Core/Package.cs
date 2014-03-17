using System;
using System.Collections.Generic;

namespace Require
{
    public class Package
    {
        public string BaseDir { get; private set; }

        public string Name { get; private set; }

        public Version Version { get; private set; }

        public List<string> Files { get; private set; }

        public List<PackageDependency> Dependencies { get; private set; }

        public PackageSpec PackageSpec { get; private set; }

        public Package(string name, Version version, string baseDir, PackageSpec packageSpec)
        {
            this.Name = name;
            this.Version = version;
            this.BaseDir = baseDir;
            this.PackageSpec = packageSpec;
            this.Files = new List<string>();
            this.Dependencies = new List<PackageDependency>();
        }
    }
}
