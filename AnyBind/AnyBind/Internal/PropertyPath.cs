using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnyBind.Internal
{
    public class PropertyPath
    {
        private PropertyPath(bool compact = false)
        {
        }
    }

    public abstract class PropertyBase : IEquatable<PropertyBase>
    {
        internal PropertyBase(string representation)
        {
            _Representation = representation;
        }

        private string _Representation;

        public override string ToString()
        {
            return _Representation;
        }

        public override bool Equals(object obj)
        {
            if (obj is PropertyBase typed)
                return Equals(typed);
            return false;
        }

        public bool Equals(PropertyBase other)
        {
            if (other == null)
                return false;
            return _Representation == other._Representation;
        }

        public override int GetHashCode()
        {
            return _Representation.GetHashCode();
        }

        public abstract bool NeedsDelimiter { get; }

        public static PropertyBase Parse(string str)
        {
            return null;
        }

        internal static bool IsValidNameChar(char chr)
        {
            return
                chr != '.' &&
                chr != ',' &&
                chr != '[' &&
                chr != ']' &&
                chr != '<' &&
                chr != '>' &&
                chr != '(' &&
                chr != ')' &&
                chr != '"';
        }

        internal static bool IsValidNamedProperty(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            var first = str.First();
            if (!(first >= 'A' && first <= 'Z') &&
                !(first >= 'a' && first <= 'z') &&
                first != '_')
                return false;
            return str.All(chr => IsValidNameChar(chr));
        }
    }

    public sealed class NamedProperty : PropertyBase
    {
        public NamedProperty(string name)
            : base(name)
        {
            if (!IsValidNamedProperty(name))
                throw new ArgumentException($"An invalid property name was given for {nameof(name)}");
        }
        public override bool NeedsDelimiter => true;
    }
}
