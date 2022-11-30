namespace DistEdu.Common;

public static class SearchOption
{
    public static bool ContainsAcceptableSymbols(char inputChar)
    {
        return inputChar is '-' or '/' or '.';
    }
}