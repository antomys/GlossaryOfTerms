using System.Diagnostics;
using System.Linq.Expressions;
using DistEdu.Common;
using DistEdu.Common.Extensions;

namespace DistEdu.CoordinatedInvertedIndex;

public sealed class CoordinatedIndexService : ServiceBase
{
    private static readonly
        Expression<Func<IEnumerable<CoordinatedIndex>, Dictionary<string, CoordIndexToken[]>>> IndicesExpression
            = invertedIndices
                => invertedIndices
                    .SelectMany(invertedIndex => invertedIndex)
                    .GroupBy(keyValuePair => keyValuePair.Key)
                    .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(x => x.Value).ToArray());

    private static readonly
        Func<IEnumerable<CoordinatedIndex>, Dictionary<string, CoordIndexToken[]>> ProcessIndices =
            IndicesExpression.Compile();

    private CoordinatedIndexes _coordinatedIndex;

    public CoordinatedIndexService(string folderName, string filesExtension)
        : base(folderName, filesExtension, nameof(CoordinatedIndexService))
    {
    }

    public async Task<CoordinatedIndexes> GetOrBuildAsync()
    {
        var files = Directory.GetFiles(OutputPath);
        if (files.Length is 0)
        {
            Console.WriteLine("Save directory is empty, building index...");
            
            return await BuildIndexAsync();
        }

        foreach (var file in files)
        {
            Console.WriteLine($"Found file : {file}. Trying to read");

            CoordinatedIndexes? index;
            if (file.EndsWith(".json"))
            {
                index = await IoExtensions.ReadTextAsync<CoordinatedIndexes>(file);

                if (index is not null)
                {
                    Console.WriteLine("Successfully read. Proceeding...\n");
                    _coordinatedIndex = index;
                    
                    return index;
                }
                
                Console.WriteLine($"Unable to read from {file}");
            }
            else if (file.EndsWith(".msgpack"))
            {
                index = await IoExtensions.ReadMsgPackAsync<CoordinatedIndexes>(file);
                
                if (index is not null)
                {
                    Console.WriteLine("Successfully read. Proceeding...\n");
                    _coordinatedIndex = index;
                    
                    return index;
                }
                
                Console.WriteLine($"Unable to read from {file}");
            }
        }

        Console.WriteLine("Unable to read, building index...");
        
        return await BuildIndexAsync();
    }

    public async Task<CoordinatedIndexes> BuildIndexAsync()
    {
        var filesInFolder = GetFiles();

        if (filesInFolder.Length is 0) return null;

        var fb2Files = filesInFolder.Select(ProcessInternalAsync);

        Console.WriteLine("~~~ Start indexing ~~~");

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var invertedIndexesArray = await Task.WhenAll(fb2Files);
        var oneIndex = Merge(invertedIndexesArray);
        
        stopWatch.Stop();
        Console.WriteLine($"> Indexing is finished in {stopWatch.ElapsedMilliseconds} ms");
        
        _coordinatedIndex = oneIndex;

        return _coordinatedIndex;
    }

    private static Task<CoordinatedIndex> ProcessInternalAsync(string fileName, int index)
    {
        return IoExtensions
            .ReadFb2FileAsync(fileName, index)
            .ContinueWith(task => new CoordinatedIndexFb2Processor(task.Result).GetIndex(),
                TaskContinuationOptions.AttachedToParent);
    }

    public Task SaveIndexAsync()
    {
        return Task.WhenAll(_coordinatedIndex.WriteJsonFileV2Async(OutputPath, nameof(CoordinatedIndex)),
            _coordinatedIndex.WriteMsgPackFileAsync(OutputPath, nameof(CoordinatedIndex)));
    }

    private static CoordinatedIndexes Merge(IEnumerable<CoordinatedIndex> invertedIndices)
    {
        var groupedIndices = ProcessIndices(invertedIndices);
        var coordIndexes = new CoordinatedIndexes
        {
            Indexes = groupedIndices
        };
        
        return coordIndexes;
    }
}