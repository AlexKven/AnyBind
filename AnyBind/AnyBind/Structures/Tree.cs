using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AnyBind.Structures
{
    class Tree<T> : Collection<TreeNode<T>> where T : IEquatable<T>
    {
        public IEnumerable<T> FindAllBranches(T from)
        {
            List<T> result = new List<T>();

            void traverseNode(TreeNode<T> node)
            {
                foreach (var sub in node)
                {
                    if (!result.Contains(sub))
                    {
                        result.Add(sub);
                        search(sub);
                    }
                }
            }

            void search(T item)
            {
                foreach (var node in this)
                {
                    if (node.Identifier.SafeEquals(item))
                        traverseNode(node);
                }
            }

            search(from);
            return result;
        }
    }
}
