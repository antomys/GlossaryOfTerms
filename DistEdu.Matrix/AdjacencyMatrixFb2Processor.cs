using System.Collections.Concurrent;
using DistEdu.Common;
using DistEdu.Common.Models;

namespace DistEdu.Matrix;

public sealed class AdjacencyMatrixFb2Processor : Fb2Processor<ConcurrentDictionary<string, HashSet<string>>>
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _lexemes;

    public AdjacencyMatrixFb2Processor(Fb2FileWrapper fb2FileWrapperWrapper)
        : base(fb2FileWrapperWrapper, (str, wrapper, dictionary) => ProcessMatrixString(str, wrapper, dictionary))
    {
        _lexemes = new ConcurrentDictionary<string, HashSet<string>>();
    }

    public ConcurrentDictionary<string, HashSet<string>> ProcessFb2WithMatrixAsync()
    {
        ProcessBodies(Fb2FileWrapper.File.Bodies, _lexemes);

        return _lexemes;
    }

    private static void ProcessMatrixString(string str, Fb2FileWrapper fileWrapper,
        ConcurrentDictionary<string, HashSet<string>> lexemeCollection)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        var subStrings = RegexExtensions.LexemeRegexV2.Split(str);

        foreach (var splitStr in subStrings.AsSpan())
        {
            if (string.IsNullOrWhiteSpace(splitStr))
            {
                continue;
            }

            var strToAdd = splitStr.ToLower();

            lexemeCollection.AddOrUpdate(fileWrapper.FileName, _ => new HashSet<string> { strToAdd }, (_, hashSet) =>
            {
                hashSet.Add(strToAdd);
               
                return hashSet;
            });
        }
    }
}