using DistEdu.CoordinatedInvertedIndex;

namespace DistEdu.CoordSearch;

public sealed class CoordInverseIndexSearchService
{
    private readonly CoordinatedIndexes _coordinatedIndexes;

    public CoordInverseIndexSearchService(CoordinatedIndexes coordinatedIndexes)
    {
        _coordinatedIndexes = coordinatedIndexes;
    }

    public string[] Search(string? query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return Array.Empty<string>();
        }

        var queryArray = query.ToLower().Split(' ');

        var tmp = new List<CoordIndexToken>();

        bool isInitial = true;

        foreach (var queryValue in queryArray)
        {
            if (!_coordinatedIndexes.Indexes.TryGetValue(queryValue, out var coordTokens))
            {
                return Array.Empty<string>();
            }

            if (isInitial)
            {
                tmp.AddRange(coordTokens);
                isInitial = false;
                
                continue;
            }

            foreach (var coordToken in coordTokens)
            {
                var containedCoord = tmp
                    .FirstOrDefault(x => x.FileName == coordToken.FileName);

                if (containedCoord is null)
                {
                    continue;
                }

                var pairs = new HashSet<int>();
                foreach (var positionIndex in containedCoord.PositionInFile)
                {
                    pairs.UnionWith(coordToken.PositionInFile
                        .Where(x => x == positionIndex + 1 || x == positionIndex - 1)
                        .Select(x => new[]{positionIndex, x})
                        .SelectMany(x=> x)
                        .ToHashSet());
                }

                if (pairs.Count is 0)
                {
                    tmp.Remove(containedCoord);
                }
                else
                {
                    containedCoord.PositionInFile = pairs;
                }
            }

        }
        
        return tmp
            .Select(x =>
                $"File: {x.FileName}. Phrase: {query}. Indexes: [{string.Join(',', x.PositionInFile.Select(z => z.ToString()))}]")
            .ToArray();
    }
}