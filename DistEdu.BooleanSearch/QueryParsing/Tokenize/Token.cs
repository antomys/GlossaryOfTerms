namespace DistEdu.BooleanSearch.QueryParsing.Tokenize;

public enum Token
{
    Term,
    And = '&',
    Or = '|',
    Not = '!',
    OpenBracket = '(',
    CloseBracket = ')',
    Eol,
    All,
    Zero
}