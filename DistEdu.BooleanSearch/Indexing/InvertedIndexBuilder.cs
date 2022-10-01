using DistEdu.BooleanSearch.DataSource;

namespace DistEdu.BooleanSearch.Indexing;

/// <summary>
///     Build a simple inverted index on file.
/// </summary>
public class InvertedIndexBuilder : IInvertedIndexBuilder
{
    public IIndex BuildIndex(IBookDataSource dataSource)
    {
        var index = new InvertedIndex();

        string line;

        var notebooks = dataSource.GetAllBooks();

        foreach (var item in notebooks)
        {
            // We look on brand and model same way, just because we don't need any ranging yet
            line = item.Value.FileName + "," + item.Value.Token;

            var termStart = 0;

            var i = 0;

            while (true)
            {
                if (char.IsLetterOrDigit(line[i]) || SearchOption.AcceptableSymbols.Contains(line[i]))
                {
                    termStart++;
                }
                else
                {
                    if (termStart > 0) index.Add(line.Substring(i - termStart, termStart).ToLower(), item.Key);

                    termStart = 0;
                }

                if (++i == line.Length)
                {
                    if (termStart > 0) index.Add(line.Substring(i - termStart, termStart).ToLower(), item.Key);

                    break;
                }
            }
        }

        return index;
    }
}