using System.Text.RegularExpressions;

namespace DistEdu.Common;

public static class RegexExtensions
{
    public static readonly Regex LexemeRegex = new("\\w+", RegexOptions.Compiled);
    
    public static readonly Regex LexemeRegexV2 = new(@"[^\p{L}]*\p{Z}[^\p{L}]*", RegexOptions.Compiled);
}