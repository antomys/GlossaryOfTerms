using DistEdu.BooleanSearch.QueryParsing.Tokenize;

namespace DistEdu.BooleanSearch.QueryParsing;

/// <summary>
///     Very simple query parser.
///     Return just a list with terms and operands together.
///     Don't understand brackets.
/// </summary>
public class SimpleQueryParser
{
    private readonly TokenReader _tokenReader;

    public SimpleQueryParser(string str)
    {
        _tokenReader = new TokenReader(str);
    }

    public string[] Parse()
    {
        var result = new List<string>();

        var waitingTermOrNot = true;
        var waitingOperation = false;
        while (true)
        {
            _tokenReader.NextToken();

            if (_tokenReader.Token == Token.EOL)
            {
                if (waitingTermOrNot) throw new Exception("Invalid search string format");

                break;
            }

            if (waitingTermOrNot)
            {
                if (_tokenReader.Token == Token.Term)
                {
                    result.Add(_tokenReader.Term);

                    waitingTermOrNot = false;
                    waitingOperation = true;
                }
                else if (_tokenReader.Token == Token.Not)
                {
                    result.Add("NOT");
                }
                else
                {
                    throw new Exception("Invalid search string format");
                }
            }
            else if (waitingOperation)
            {
                if (_tokenReader.Token == Token.And)
                    result.Add("AND");
                else if (_tokenReader.Token == Token.Or)
                    result.Add("OR");
                else
                    throw new Exception("Invalid search string format");

                waitingTermOrNot = true;
                waitingOperation = false;
            }
        }

        return result.ToArray();
    }
}