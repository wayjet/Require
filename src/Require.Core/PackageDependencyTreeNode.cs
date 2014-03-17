using System;
using System.Collections.Generic;

namespace Require
{
    public class PackageDependencyTreeNode : TreeNode<PackageDependencyTreeNode, Package>
    {
        public PackageDependencyTreeNode()
            : base()
        { }

        public PackageDependencyTreeNode(Package package)
            : this()
        {
            this.Data = package;
        }
    }
}
