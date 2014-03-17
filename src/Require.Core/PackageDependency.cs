
using Newtonsoft.Json;
using System;

namespace Require
{
    public class PackageDependency
    {
        public string Name { get; set; }

        public DependencyVersion MinVersion { get; set; }

        public DependencyVersion MaxVersion { get; set; }
    }

}
