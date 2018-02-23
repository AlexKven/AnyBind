using System;
using System.Collections.Generic;
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
    }
}
