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

            foreach (var tmpArr in tmp)
            {
                var containedCoord = coordTokens
                    .FirstOrDefault(x => x.FileName == tmpArr.FileName);

                if (containedCoord is null)
                {
                    tmpArr.IsRemoved = true;
                    continue;
                }

                var pairs = new HashSet<int>();
                
                foreach (var positionIndex in containedCoord.PositionInFile)
                {
                    pairs.UnionWith(tmpArr.PositionInFile
                        .Where(x => x == positionIndex + 1 || x == positionIndex - 1)
                        .Select(x => new[]{positionIndex, x})
                        .SelectMany(x=> x)
                        .ToHashSet());
                }

                if (pairs.Count is 0)
                {
                    tmpArr.IsRemoved = true;
                }
                else
                {
                    tmpArr.Found = pairs;
                }
            }
        }
        
        return tmp
            .Where(x=> !x.IsRemoved)
            .Select(x =>
                $"File: {x.FileName}. Phrase: {query}. Indexes: [{string.Join(',', x.Found.Select(z => z.ToString()))}]")
            .ToArray();
    }
}