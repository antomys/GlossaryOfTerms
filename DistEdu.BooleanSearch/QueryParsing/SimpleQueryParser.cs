using DistEdu.BooleanSearch.QueryParsing.Tokenize;

namespace DistEdu.BooleanSearch.QueryParsing;

/// <summary>
///     Very simple query parser.
///     Return just a list with terms and operands together.
///     Don't understand brackets.
/// </summary>
public sealed class SimpleQueryParser
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

            if (_tokenReader.Token is Token.Eol)
            {
                if (waitingTermOrNot) throw new Exception("Invalid search string format");

                break;
            }

            if (waitingTermOrNot)
            {
                switch (_tokenReader.Token)
                {
                    case Token.Term:
                        result.Add(_tokenReader.Term);

                        waitingTermOrNot = false;
                        waitingOperation = true;
                        break;
                    case Token.Not:
                        result.Add("NOT");
                        break;
                    default:
                        throw new Exception("Invalid search string format");
                }
            }
            else if (waitingOperation)
            {
                switch (_tokenReader.Token)
                {
                    case Token.And:
                        result.Add("AND");
                        break;
                    case Token.Or:
                        result.Add("OR");
                        break;
                    default:
                        throw new Exception("Invalid search string format");
                }

                waitingTermOrNot = true;
                waitingOperation = false;
            }
        }

        return result.ToArray();
    }
}