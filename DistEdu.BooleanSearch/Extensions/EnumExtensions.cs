using DistEdu.BooleanSearch.QueryParsing.Tokenize;

namespace DistEdu.BooleanSearch.Extensions;

public static class EnumExtensions
{
    public static string GetName(this Token token)
    {
        return token switch
        {
            Token.And => "AND",
            Token.Or => "OR",
            Token.Not => "NOT",
            Token.CloseBracket => ")",
            Token.OpenBracket => "(",
            Token.All => "ALL",
            Token.Zero => "ZERO",
            _ => Enum.GetName(token)!
        };
    }
}