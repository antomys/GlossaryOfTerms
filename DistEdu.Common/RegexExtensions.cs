using System.Text.RegularExpressions;

namespace DistEdu.Common;

public static class RegexExtensions
{
    public static readonly Regex LexemeRegex = new("\\w+", RegexOptions.Compiled);
}