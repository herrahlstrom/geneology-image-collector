using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace GenPhoto.Helpers
{
    internal static class VolumeFormatter
    {
        static Regex DefaultRegex = new Regex(@"[^ a-zåäö0-9\.]", RegexOptions.IgnoreCase);

        static Regex[] SpecifiedRegexs = new[]
        {
            new Regex(@"^([A-F])[ :]?(\d+)\s(.+)$", RegexOptions.Compiled),
            new Regex(@"^([A-F])\s?(I+)[ :](.+)$", RegexOptions.Compiled),
            new Regex(@"^([A-F])\s?(I+)\s?([a])[ :](.+)$", RegexOptions.Compiled)
        };

        public static string GetFormattedValue(string input)
        {
            foreach (var rx in SpecifiedRegexs)
            {
                var match = rx.Match(input);
                if (match.Success)
                {
                    StringBuilder result = new(input.Length + 10);

                    result.Append(match.Groups[1].Value);
                    for (int i = 2; i < match.Groups.Count; i++)
                    {
                        result.AppendFormat(" {0}", match.Groups[i].Value);
                    }
                    return result.ToString();
                }
            }

            return DefaultRegex.Replace(input, " ");
        }
    }
}
