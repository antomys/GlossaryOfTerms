namespace DistEdu.CoordSearch;

public sealed class PhrasalSearchService
{
    private readonly DoubleIndex.DoubleIndex _doubleIndex;

    public PhrasalSearchService(DoubleIndex.DoubleIndex doubleIndex)
    {
        _doubleIndex = doubleIndex;
    }

    public string[] Search(string? query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return Array.Empty<string>();
        }

        query = query.ToLower();
        var foundPhrases = _doubleIndex
            .Where(x => x.Value.Contains(query))
            .Select(x => $"File: {x.Key}. Phrase: {x.Value.FirstOrDefault(y=> y == query)}")
            .ToArray();
        
        return foundPhrases;
    }
}