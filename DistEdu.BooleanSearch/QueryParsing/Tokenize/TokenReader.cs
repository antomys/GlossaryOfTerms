using DistEdu.Common.Extensions;
using SearchOption = DistEdu.Index.SearchOption;

namespace DistEdu.BooleanSearch.QueryParsing.Tokenize;

public sealed class TokenReader
{
    private readonly ReadOnlyMemory<char> _query;
    private char _currentChar = char.MaxValue;
    private int _position;

    public TokenReader(string query)
    {
        _query = query.AsMemory();
    }

    public Token Token { get; private set; } = Token.Eol;

    public Token PrevToken { get; private set; } = Token.Eol;

    public string Term { get; private set; } = string.Empty;

    public void NextToken()
    {
        PrevToken = Token;

        if (_currentChar is char.MaxValue) NextChar();

        SkipWhitespaces();

        switch (_currentChar)
        {
            case char.MinValue:
                Token = Token.Eol;
                return;

            case (char)Token.And:
                NextChar();
                if (_currentChar == (char)Token.And) NextChar();

                Token = Token.And;
                return;

            case (char)Token.Or:
                NextChar();
                if (_currentChar == (char)Token.Or) NextChar();

                Token = Token.Or;
                return;

            case (char)Token.Not:
                // We consider space(s) between term and NOT as AND operation.
                if (PrevToken == Token.Term)
                {
                    Token = Token.And;
                    return;
                }

                NextChar();
                Token = Token.Not;
                return;

            case (char)Token.OpenBracket:
                NextChar();
                Token = Token.OpenBracket;
                return;

            case (char)Token.CloseBracket:
                NextChar();
                Token = Token.CloseBracket;
                return;
        }

        if (char.IsLetterOrDigit(_currentChar) || SearchOption.ContainsAcceptableSymbols(_currentChar))
        {
            // We consider space(s) between terms as AND operation.
            if (PrevToken == Token.Term)
            {
                Token = Token.And;

                return;
            }

            var i = _position - 1;

            while ((char.IsLetterOrDigit(_currentChar) || SearchOption.ContainsAcceptableSymbols(_currentChar))
                   && _currentChar != char.MinValue)
                NextChar();

            var term = _query.Span.Slice(i, _position - i - (_currentChar == char.MinValue ? 0 : 1));
            Term = term.Lower();
            Token = Token.Term;

            return;
        }

        throw new Exception($"Unexpected character: {_currentChar}");
    }

    private void NextChar()
    {
        if (_position >= _query.Length)
        {
            _currentChar = char.MinValue;
            return;
        }

        _currentChar = _query.Span[_position++];
    }

    private void SkipWhitespaces()
    {
        while (char.IsWhiteSpace(_currentChar)) NextChar();
    }
}