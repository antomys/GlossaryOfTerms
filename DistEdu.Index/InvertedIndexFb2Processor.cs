using DistEdu.Common;
using DistEdu.Common.Extensions;
using DistEdu.Common.Models;
using DistEdu.Index.Interfaces;
using SearchOption = DistEdu.Common.SearchOption;

namespace DistEdu.Index;

public sealed class InvertedIndexFb2Processor : Fb2Processor<IIndex>
{
    private static int _id;
    private readonly InvertedIndex _index;

    public InvertedIndexFb2Processor(Fb2FileWrapper fb2FileWrapperWrapper)
        : base(fb2FileWrapperWrapper, (str, wrapper, collection) => AddToIndex(str, wrapper, collection))
    {
        _index = new InvertedIndex();
    }

    public InvertedIndex GetIndex()
    {
        ProcessBodies(Fb2FileWrapper.File.Bodies, _index);

        return _index;
    }

    private static void AddToIndex(string stringToProcess, Fb2FileWrapper fileWrapper, IIndex invertedIndex)
    {
        if (string.IsNullOrEmpty(stringToProcess)) return;

        var termStart = 0;
        var i = 0;
        var stringSpan = stringToProcess.AsSpan();

        while (true)
        {
            if (char.IsLetterOrDigit(stringSpan[i]) || SearchOption.ContainsAcceptableSymbols(stringSpan[i]))
            {
                termStart++;
            }
            else
            {
                if (termStart > 0)
                {
                    var slice = stringSpan.Slice(i - termStart, termStart);
                    invertedIndex.Add(slice.Lower(), BuildKey(fileWrapper.FileIndex, fileWrapper.FileName, ref _id));
                }

                termStart = 0;
            }

            if (++i != stringToProcess.Length) continue;

            if (termStart > 0)
            {
                var slice = stringSpan.Slice(i - termStart, termStart);

                invertedIndex.Add(slice.Lower(), BuildKey(fileWrapper.FileIndex, fileWrapper.FileName, ref _id));
            }

            break;
        }
    }

    private static string BuildKey(int fileId, string fileName, ref int id)
    {
        var termId = $"{fileId}:{fileName}:{id}";
        Interlocked.Increment(ref id);

        return termId;
    }
}