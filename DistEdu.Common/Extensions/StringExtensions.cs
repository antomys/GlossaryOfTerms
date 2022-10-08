using System.Globalization;
using System.Text;

namespace DistEdu.Common.Extensions;

public static class StringExtensions
{
    public static string Lower(this ref ReadOnlySpan<char> span)
    {
        Span<char> tmpArray = stackalloc char[span.Length];
        span.ToLower(tmpArray, CultureInfo.CurrentCulture);

        tmpArray = tmpArray.Trim();

        return tmpArray.ToString();
    }

    public static string GetString<T>(this IEnumerable<T> values)
    {
        var sb = new StringBuilder();
        sb.AppendJoin(';', values);

        return sb.ToString();
    }
}