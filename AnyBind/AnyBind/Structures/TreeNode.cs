using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AnyBind.Structures
{
    internal class TreeNode<T> : Collection<T> where T : IEquatable<T>
    {
        public T Identifier { get; set; }

        public TreeNode() { }
        public TreeNode(T identifier) => Identifier = identifier;
    }
}
