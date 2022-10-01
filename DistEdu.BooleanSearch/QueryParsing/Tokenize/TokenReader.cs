namespace DistEdu.BooleanSearch.QueryParsing.Tokenize;

public class TokenReader
{
    private const char AND = '&';
    private const char OR = '|';
    private const char NOT = '!';
    private const char OPEN_BRACKET = '(';
    private const char CLOSE_BRACKET = ')';
    private readonly string _query;
    private char _currentChar = char.MaxValue;
    private int _position;

    public TokenReader(string query)
    {
        _query = query;
    }

    public Token Token { get; private set; } = Token.EOL;

    public Token PrevToken { get; private set; } = Token.EOL;

    public string Term { get; private set; }

    public void NextToken()
    {
        PrevToken = Token;

        if (_currentChar == char.MaxValue) NextChar();

        SkipWhitespaces();

        switch (_currentChar)
        {
            case char.MinValue:
                Token = Token.EOL;
                return;

            case AND:
                NextChar();
                if (_currentChar == AND) NextChar();

                Token = Token.And;
                return;

            case OR:
                NextChar();
                if (_currentChar == OR) NextChar();

                Token = Token.Or;
                return;

            case NOT:
                // We consider space(s) between term and NOT as AND operation.
                if (PrevToken == Token.Term)
                {
                    Token = Token.And;
                    return;
                }

                NextChar();
                Token = Token.Not;
                return;

            case OPEN_BRACKET:
                NextChar();
                Token = Token.OpenBracket;
                return;

            case CLOSE_BRACKET:
                NextChar();
                Token = Token.CloseBracket;
                return;
        }

        if (char.IsLetterOrDigit(_currentChar) || SearchOption.AcceptableSymbols.Contains(_currentChar))
        {
            // We consider space(s) between terms as AND operation.
            if (PrevToken == Token.Term)
            {
                Token = Token.And;

                return;
            }

            var i = _position - 1;

            while ((char.IsLetterOrDigit(_currentChar) || SearchOption.AcceptableSymbols.Contains(_currentChar))
                   && _currentChar != char.MinValue)
                NextChar();

            Term = _query.Substring(i, _position - i - (_currentChar == char.MinValue ? 0 : 1)).ToLower();
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

        _currentChar = _query[_position++];
    }

    private void SkipWhitespaces()
    {
        while (char.IsWhiteSpace(_currentChar)) NextChar();
    }
}