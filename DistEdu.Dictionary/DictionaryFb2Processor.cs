using System.Collections.Concurrent;
using DistEdu.Common;
using DistEdu.Common.Models;

namespace DistEdu.Dictionary;

public sealed class DictionaryFb2Processor : Fb2Processor<ConcurrentDictionary<string, int>>
{
    private readonly ConcurrentDictionary<string, int> _glossaryOfTerms;

    public DictionaryFb2Processor(Fb2FileWrapper fb2FileWrapperWrapper,
        ConcurrentDictionary<string, int> glossaryOfTerms)
        : base(fb2FileWrapperWrapper, (str, _, collection) => ProcessString(str, collection))
    {
        _glossaryOfTerms = glossaryOfTerms;
    }

    public Task ProcessFb2()
    {
        ProcessBodies(Fb2FileWrapper.File.Bodies, _glossaryOfTerms);

        return Task.CompletedTask;
    }

    private static void ProcessString(string str, ConcurrentDictionary<string, int> glossaryOfTerms)
    {
        if (string.IsNullOrWhiteSpace(str)) return;

        var subStrings = RegexExtensions.LexemeRegex.Matches(str).Select(x => x.Value);

        foreach (var splitStr in subStrings)
            glossaryOfTerms.AddOrUpdate(splitStr.ToLower(), _ => 1, (_, i) => Interlocked.Increment(ref i));
    }
}