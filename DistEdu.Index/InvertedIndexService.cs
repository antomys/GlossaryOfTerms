using System.Diagnostics;
using System.Linq.Expressions;
using DistEdu.Common;
using DistEdu.Common.Extensions;

namespace DistEdu.Index;

public sealed class InvertedIndexService : ServiceBase
{
    private static readonly
        Expression<Func<IEnumerable<InvertedIndex>, Dictionary<string, HashSet<string>>>> IndicesExpression
            = invertedIndices
                => invertedIndices
                    .SelectMany(invertedIndex => invertedIndex)
                    .GroupBy(keyValuePair => keyValuePair.Key)
                    .ToDictionary(grouping => grouping.Key,
                        grouping => grouping.SelectMany(keyValuePair => keyValuePair.Value).ToHashSet());

    private static readonly
        Func<IEnumerable<InvertedIndex>, Dictionary<string, HashSet<string>>> ProcessIndices =
            IndicesExpression.Compile();

    private InvertedIndex _index;

    public InvertedIndexService(string folderName, string filesExtension)
        : base(folderName, filesExtension, nameof(InvertedIndexService))
    {
    }

    public async Task<InvertedIndex?> GetOrBuildAsync()
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

            InvertedIndex? index;
            if (file.EndsWith(".json"))
            {
                index = await IoExtensions.ReadTextAsync<InvertedIndex>(file);

                if (index is not null)
                {
                    Console.WriteLine("Successfully read. Proceeding...\n");
                    _index = index;
                    
                    return index;
                }
                
                Console.WriteLine($"Unable to read from {file}");
            }
            else if (file.EndsWith(".msgpack"))
            {
                index = await IoExtensions.ReadMsgPackAsync<InvertedIndex>(file);
                
                if (index is not null)
                {
                    Console.WriteLine("Successfully read. Proceeding...\n");
                    _index = index;
                    
                    return index;
                }
                
                Console.WriteLine($"Unable to read from {file}");
            }
        }

        Console.WriteLine("Unable to read, building index...");
        
        return await BuildIndexAsync();
    }

    public async Task<InvertedIndex?> BuildIndexAsync()
    {
        var filesInFolder = GetFiles();

        if (filesInFolder.Length is 0) return null;

        var fb2Files = filesInFolder.Select((file, index) => ProcessInternalAsync(file, index));

        Console.WriteLine("~~~ Start indexing ~~~");

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var invertedIndexesArray = await Task.WhenAll(fb2Files);
        var oneIndex = Merge(invertedIndexesArray);

        stopWatch.Stop();
        Console.WriteLine($"> Indexing is finished in {stopWatch.ElapsedMilliseconds} ms");

        _index = oneIndex;

        return _index;
    }

    private static Task<InvertedIndex> ProcessInternalAsync(string fileName, int index)
    {
        return IoExtensions
            .ReadFb2FileAsync(fileName, index)
            .ContinueWith(task => new InvertedIndexFb2Processor(task.Result).GetIndex(),
                TaskContinuationOptions.AttachedToParent);
    }

    public void GetStatistics()
    {
        Console.WriteLine();
        Console.WriteLine("\t\t~~~~ STATISTICS: ~~~~");

        Console.WriteLine();
        Console.WriteLine($">\n>Keys: {_index.Keys.Count};\n>Values: {_index.Values.Sum(value => value.Count)}");
        Console.WriteLine($">Inverted index terms size: {_index.Size()}.");
        Console.WriteLine();

        Console.WriteLine("> Top 10 most popular lexemes: ");

        var top10Lexemes = _index
            .OrderByDescending(kvp => kvp.Value.Count)
            .Take(10);

        var index = 1;

        foreach (var kvp in top10Lexemes)
            Console.WriteLine($"\t> {index++}. Lexeme: {kvp.Key}; Times: {kvp.Value.Count};");

        var inputFileNames = IoExtensions.GetFileNames(FolderName, FilesExtension);
        Console.WriteLine("> List of processed files and their sizes:");

        if (inputFileNames.Length is 0) return;

        PrintFilesInfo(inputFileNames);

        var outputFileNames = Directory.GetFiles(OutputPath);

        if (outputFileNames.Length is 0) return;

        Console.WriteLine("> List of saved files and their sizes:");
        PrintFilesInfo(outputFileNames);
    }

    public Task SaveIndexAsync()
    {
        return Task.WhenAll(_index.WriteJsonFileV2Async(OutputPath, nameof(InvertedIndex)),
            _index.WriteMsgPackFileAsync(OutputPath, nameof(InvertedIndex)));
    }

    private static InvertedIndex Merge(IEnumerable<InvertedIndex> invertedIndices)
    {
        var newIndex = new InvertedIndex();

        var groupedIndices = ProcessIndices(invertedIndices);

        foreach (var keyValuePair in groupedIndices)
        {
            newIndex.Add(keyValuePair.Key, keyValuePair.Value);
        }

        return newIndex;
    }
}