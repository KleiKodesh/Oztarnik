using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Otzarnik.Search
{
    public static class RegexExtension
    {
        public static IEnumerable<Match> EnumerateMatches(this Regex regex, string input)
        {
            for (var match = regex.Match(input); match.Success; match = match.NextMatch())
                yield return match;
        }
    }
}
