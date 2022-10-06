using DistEdu.BooleanSearch.DataSource;
using DistEdu.BooleanSearch.Indexing;
using DistEdu.BooleanSearch.QueryParsing;

namespace DistEdu.BooleanSearch.Searching;

public sealed class SimpleSearcher
{
    private readonly IBookDataSource _dataSource;
    private readonly IIndex _index;

    public SimpleSearcher(IIndex index, IBookDataSource dataSource)
    {
        _index = index;
        _dataSource = dataSource;
    }

    public List<int> Search(string query)
    {
        var parser = new SimpleQueryParser(query);
        var parsedQuery = parser.Parse();

        if (parsedQuery.Length is 1) return _index.Find(parsedQuery[0]);

        var i = 0;
        var result = new List<int>();
        while (i < parsedQuery.Length)
        {
            if (i is 0)
            {
                var invertedFirstArg = parsedQuery[i] is "NOT";

                i = invertedFirstArg ? ++i : i;

                result = invertedFirstArg 
                    ? _dataSource
                        .GetAllIds()
                        .Except(_index.Find(parsedQuery[i++]))
                        .ToList() 
                    : _index.Find(parsedQuery[i++]);
            }

            switch (parsedQuery[i])
            {
                case "AND":
                    i++;

                    result = parsedQuery[i] is "NOT"
                        ? result
                            .Except(_index.Find(parsedQuery[++i]))
                            .ToList() 
                        : result
                            .Intersect(_index.Find(parsedQuery[i]))
                            .ToList();
                    break;
                case "OR":
                    i++;

                    result = parsedQuery[i] is "NOT" 
                        ? result
                            .Concat(_dataSource.GetAllIds()
                                .Except(_index.Find(parsedQuery[++i])))
                            .ToList() 
                        : result
                            .Concat(_index.Find(parsedQuery[i]))
                            .ToList();
                    break;
            }

            i++;
        }

        return result;
    }
}