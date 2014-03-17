using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Require
{
	/// <summary>
	/// 树节点接口
	/// </summary>
	/// <typeparam name="T">树节点类型</typeparam>
    public class TreeNode<TNode, TData> 
        where TNode : TreeNode<TNode, TData>, new()
	{
        public TreeNode()
        {
            this.Children = new TreeNodeCollection<TNode, TData>((TNode)this);
        }

		/// <summary>
		/// 获取父节点
		/// </summary>
        [JsonIgnore]
        public TNode Parent { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public TData Data { get; protected internal set; }

		/// <summary>
		/// 获取所有子节点
		/// </summary>
        public TreeNodeCollection<TNode, TData> Children { get; private set; }

        public IEnumerable<TNode> Find(Predicate<TNode> match, bool checkSelf)
        {
            if (match == null) throw new ArgumentNullException("match");

            var result = new List<TNode>();
            TNode obj = (TNode)this;
            if (checkSelf && match(obj)) result.Add(obj);

            Stack<TNode> stack = new Stack<TNode>();
            foreach (var child in obj.Children)
            {
                stack.Push(child);
            }

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (match(node)) result.Add(node);
                foreach (var child in node.Children)
                {
                    stack.Push(child);
                }
            }
            return result;
        }

        ///// <summary>
        ///// 定位到第一个符合条件的节点
        ///// </summary>
        ///// <param name="match">条件函数</param>
        ///// <returns></returns>
        //T Walk(Predicate<T> match);

        /// <summary>
        /// 深度查找符合条件的节点
        /// </summary>
        /// <param name="match"></param>
        /// <param name="checkSelf">自身节点是否也参与匹配</param>
        /// <returns></returns>
        //

        ///// <summary>
        ///// 获取节点的树路径
        ///// </summary>
        ///// <returns></returns>
        //[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        //IList<T> GetTreePaths();

        ///// <summary>
        ///// 获取节点的所有父节点
        ///// </summary>
        ///// <returns></returns>
        //[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        //IList<T> GetParents();

        ///// <summary>
        ///// 获取符合条件的节点,成功返回节点，失败返回null
        ///// </summary>
        ///// <param name="match"></param>
        ///// <returns></returns>
        //T GetParentsUntil(Predicate<T> match);
	}


    public class TreeNode<TData> : TreeNode<TreeNode<TData>, TData>
    { }
    //public class TreeNode<TData> : ITreeNode<TreeNode<TData>, TData>
    //{
    //    public TreeNode<TData> Parent { get; set; }

    //    public TreeNodeCollection<TreeNode<TData>, TData> Children { get; private set; }


    //}
}
