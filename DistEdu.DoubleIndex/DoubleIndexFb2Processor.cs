using DistEdu.Common;
using DistEdu.Common.Models;

namespace DistEdu.DoubleIndex;

public sealed class DoubleIndexFb2Processor : Fb2Processor<DoubleIndex>
{
    private readonly DoubleIndex _index;

    public DoubleIndexFb2Processor(Fb2FileWrapper fb2FileWrapperWrapper)
        : base(fb2FileWrapperWrapper, (str, wrapper, collection) => AddToIndex(str, wrapper, collection))
    {
        _index = new DoubleIndex();
    }

    public DoubleIndex GetIndex()
    {
        ProcessBodies(Fb2FileWrapper.File.Bodies, _index);

        return _index;
    }

    private static void AddToIndex(
        string stringToProcess,
        Fb2FileWrapper fileWrapper,
        DoubleIndex doubleIndex)
    {
        if (string.IsNullOrEmpty(stringToProcess))
        {
            return;
        }
        
        var localWords = RegexExtensions.LexemeRegexV2
            .Split(stringToProcess)
            .AsSpan();

        for (var i = 0; i < localWords.Length - 1; i++)
        {
            var wordPair = $"{localWords[i]} {localWords[i + 1]}".ToLower();

            doubleIndex.AddOrUpdate(fileWrapper.FileName, _ => new HashSet<string>
            {
                wordPair
            }, (_, hashSet) =>
            {
                hashSet.Add(wordPair);

                return hashSet;
            });
        }
    }
}