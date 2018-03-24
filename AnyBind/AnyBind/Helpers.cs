using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyBind
{
    static class Helpers
    {
		internal static bool TryGetTarget(this WeakReference reference, out object result)
		{
			result = null;
			if (reference.IsAlive && (result = reference.Target) != null)
				return true;
			return false;
		}

		internal static object GetTargetOrDefault(this WeakReference reference)
		{
			reference.TryGetTarget(out object result);
			return result;
		}

        internal static bool SafeEquals<T>(this IEquatable<T> left, IEquatable<T> right)
        {
            return left?.Equals(right) ?? false;
        }

        internal static List<T> FindDependencyBranches<T>(this Dictionary<T, List<T>> dependencyTree, params T[] toSearch) where T : IEquatable<T>
        {
            List<T> result = new List<T>();

            void traverseNode(List<T> items)
            {
                foreach (var sub in items)
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
                if (dependencyTree.TryGetValue(item, out var node))
                {
                    traverseNode(node);
                }
            }

            foreach (var item in toSearch)
            {
                search(item);
            }
            return result;
        }

        internal static void SafeAddToDictionaryOfList<TKey, TList>(this Dictionary<TKey, List<TList>> dict, TKey key, params TList[] items)
        {
            if (dict.TryGetValue(key, out var list))
            {
                list.AddRange(items);
            }
            else
            {
                dict.Add(key, items.ToList());
            }
        }
    }
}
