using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AnyBind.Internal
{
    public static class StringHelpers
    {
        public static IEnumerable<string> DisassemblePropertyPath(this string propertyPath)
        {
            StringBuilder sb = new StringBuilder();
            int indexerDepth = 0;
            foreach (var character in propertyPath)
            {
                if (character == '[')
                {
                    indexerDepth++;
                    if (indexerDepth == 1)
                    {
                        if (sb.Length > 0)
                            yield return sb.ToString();
                        sb = new StringBuilder();
                    }
                }
                if (indexerDepth > 0)
                {
                    if (character == ']')
                        indexerDepth--;
                    sb.Append(character);
                }
                else if (character == '.')
                {
                    if (sb.Length > 0)
                        yield return sb.ToString();
                    sb = new StringBuilder();
                }
                else
                    sb.Append(character);
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }

        public static string ReassemblePropertyPath(params string[] components) => ReassemblePropertyPath((IEnumerable<string>)components);

        public static string ReassemblePropertyPath(this IEnumerable<string> components)
        {
            if (components == null)
                return null;
            return components.Aggregate("", (acc, str) => acc == "" ? str : str.StartsWith("[") ? $"{acc}{str}" : $"{acc}.{str}");
        }

        public static string GetParentOfPropertyPath(this IEnumerable<string> components, out string propertyName)
        {
            propertyName = components?.LastOrDefault();
            return components.GetParentOfPropertyPath();
        }

        public static string GetParentOfPropertyPath(this IEnumerable<string> components)
        {
            if (components == null)
                return null;
            return components.Aggregate(("", ""), (acc, str) => (acc.Item1 == "" ? str : str.StartsWith("[") ? acc.Item1 + str : $"{acc.Item1}.{str}", acc.Item1)).Item2;
        }
        // [123456]
        public static bool GetIndexAndParent(this string propertyPath, out string parentPath, out string index)
        {
            parentPath = null;
            index = null;
            if (propertyPath.EndsWith("]"))
            {
                int i = propertyPath.Length - 2;
                while (propertyPath[i] != '[')
                {
                    i--;
                    if (i < 0)
                        return false;
                }
                index = propertyPath.Substring(i + 1, propertyPath.Length - i - 2);
                if (i > 0 && propertyPath[i - 1] == '.')
                    parentPath = propertyPath.Substring(0, i - 1); // xyz.[0]
                else
                    parentPath = propertyPath.Substring(0, i); // xyz[0]
                return true;
            }
            return false;
        }
    }
}
