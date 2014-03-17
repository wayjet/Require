using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Require
{
	/// <summary>
	/// 树节点集合类
	/// </summary>
	/// <typeparam name="TNode"></typeparam>
	public class TreeNodeCollection<TNode, TData> : Collection<TNode> 
        where TNode : TreeNode<TNode,TData>, new()
	{
		/// <summary>获取该集合的拥有者(即该集合中所有节点所共有的父节点)</summary>
		public TNode Owner { get; private set; }

		/// <summary>
		/// 构造
		/// </summary>
		/// <param name="owner">集合的拥有者</param>
		public TreeNodeCollection(TNode owner)
		{
			Owner = owner;
		}

		/// <inheritdoc />
		protected override void InsertItem(int index, TNode item)
		{
			if (item == null) throw new ArgumentNullException("item");
			base.InsertItem(index, item);
			item.Parent = Owner;
		}

		/// <inheritdoc />
		protected override void SetItem(int index, TNode item)
		{
			if (item == null) throw new ArgumentNullException("item");
			if (item.Parent == null) throw new ArgumentException("添加的节点的Parent必须为null", "item");
			base.SetItem(index, item);
			item.Parent = Owner;
		}

		/// <inheritdoc />
		protected override void RemoveItem(int index)
		{
			if (index < 0 || index > Count - 1) throw new ArgumentOutOfRangeException("index");
			var item = Items[index];
			base.RemoveItem(index);
			item.Parent = default(TNode);
		}

		/// <inheritdoc />
		protected override void ClearItems()
		{
			foreach (var item in Items) {
				item.Parent = default(TNode);
			}
			base.ClearItems();
		}

        public TNode Add(TData data)
        {
            TNode node = new TNode();
            node.Data = data;
            this.Add(node);
            return node;
        }

		/// <summary>
		/// 添加多个节点
		/// </summary>
		/// <param name="collection">要添加到树的节点，注意:必须确保节点的Parent为null</param>
		public void AddRange(IEnumerable<TNode> collection)
		{
			if (collection == null) return;
			foreach (var data in collection) {
				Add(data);
			}
		}
	}
}
