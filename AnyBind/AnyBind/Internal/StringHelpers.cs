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
    }
}
