using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using DistEdu.Common;
using DistEdu.Common.Extensions;

namespace DistEdu.DoubleIndex;

public sealed class DoubleIndexService : ServiceBase
{
    
    private static readonly
        Expression<Func<IEnumerable<DoubleIndex>, Dictionary<string, HashSet<string>>>> IndicesExpression
            = invertedIndices
                => invertedIndices
                    .SelectMany(invertedIndex => invertedIndex)
                    .GroupBy(keyValuePair => keyValuePair.Key)
                    .ToDictionary(grouping => grouping.Key,
                        grouping => grouping.SelectMany(keyValuePair => keyValuePair.Value).ToHashSet());

    private static readonly
        Func<IEnumerable<DoubleIndex>, Dictionary<string, HashSet<string>>> ProcessIndices =
            IndicesExpression.Compile();

    private DoubleIndex _index;
    private string[] _fileNames;

    public DoubleIndexService(string folderName, string filesExtension)
        : base(folderName, filesExtension, nameof(DoubleIndex))
    {
    }
    
    public async Task<DoubleIndex?> GetOrBuildAsync()
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

            DoubleIndex? index;
            if (file.EndsWith(".json"))
            {
                index = await IoExtensions.ReadTextAsync<DoubleIndex>(file);

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
                index = await IoExtensions.ReadMsgPackAsync<DoubleIndex>(file);
                
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
    
    private async Task<DoubleIndex?> BuildIndexAsync()
    {
        var filesInFolder = GetFiles();

        if (filesInFolder.Length is 0)
        {
            return null;
        }

        var fb2Files = filesInFolder.Select(ProcessInternalAsync);

        Console.WriteLine("~~~ Start indexing. Double index ~~~");

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var doubleIndexes = await Task.WhenAll(fb2Files);
        var oneIndex = Merge(doubleIndexes);
        _fileNames = doubleIndexes
            .Select(task => (task.Keys, task))
            .SelectMany(valueTuple => valueTuple.Keys)
            .ToArray();

        stopWatch.Stop();
        Console.WriteLine($"> Indexing is finished in {stopWatch.ElapsedMilliseconds} ms");

        _index = oneIndex;

        return _index;
    }
    
    public Task SaveIndexAsync()
    {
        return Task.WhenAll(
            _index.WriteJsonFileV2Async(OutputPath, nameof(DoubleIndex)),
            _index.WriteMsgPackFileAsync(OutputPath, nameof(DoubleIndex)),
            WritePrettyDoubleIndexAsync());
    }

    public async Task WritePrettyDoubleIndexAsync()
    {
        if (_index.Count is 0)
        {
            Console.WriteLine("Unable to write, Double Index is empty");

            return;
        }

        const string fileNameDir = "DoubleIndex_Pretty.csv";

        var file = Path.Combine(OutputPath, fileNameDir);

        Console.WriteLine($"Double Index async Write {fileNameDir} File has started.");

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        await using (var outputFile = new StreamWriter(file, false, Encoding.UTF8))
        {
            var headers = $"Lexeme;{string.Join(';', _fileNames)}";
            await outputFile.WriteLineAsync(headers);

            var lines = GetMatrix(_index);
            await outputFile.WriteLineAsync(lines);
        }

        Console.WriteLine($"Double Index async Write {fileNameDir} File has completed.");
    }
    
    public void GetStatistics()
    {
        Console.WriteLine();
        Console.WriteLine("\t\t~~~~ STATISTICS: ~~~~");

        Console.WriteLine();
        Console.WriteLine($">\n>Keys: {_index.Keys.Count};\n>Values: {_index.Values.Sum(value => value.Count)}");
        Console.WriteLine($">Double index terms size: {_index.Count}.");
        Console.WriteLine();

        var inputFileNames = IoExtensions.GetFileNames(FolderName, FilesExtension);
        Console.WriteLine("> List of processed files and their sizes:");

        if (inputFileNames.Length is 0)
        {
            return;
        }

        PrintFilesInfo(inputFileNames);

        var outputFileNames = Directory.GetFiles(OutputPath);

        if (outputFileNames.Length is 0) return;

        Console.WriteLine("> List of saved files and their sizes:");
        PrintFilesInfo(outputFileNames);
    }
    
    private static Task<DoubleIndex> ProcessInternalAsync(string fileName, int index)
    {
        return IoExtensions
            .ReadFb2FileAsync(fileName, index)
            .ContinueWith(task => new DoubleIndexFb2Processor(task.Result).GetIndex(),
                TaskContinuationOptions.AttachedToParent);
    }
    
    private static DoubleIndex Merge(IEnumerable<DoubleIndex> invertedIndices)
    {
        var newDoubleIndex = new DoubleIndex();

        var groupedIndices = ProcessIndices(invertedIndices);
        
        foreach (var (fileName, terms) in groupedIndices)
        {
            newDoubleIndex.AddOrUpdate(
                fileName, 
                _ => terms,
                (_, list) =>
                {
                    list.UnionWith(terms);

                    return list;
                });
        }
        
        return newDoubleIndex;
    }

    private static string GetMatrix(DoubleIndex doubleIndex)
    {
        var index = 0;

        var lexemes = doubleIndex.Values
            .SelectMany(x=> x)
            .ToArray()
            .AsSpan();
        
        var matrixLine = new string[lexemes.Length];

        foreach (ref readonly var lexeme in lexemes)
        {
            var internalMatrix = new string[doubleIndex.Count];
            var i = 0;
            
            foreach (var (k, v) in doubleIndex)
            {
                if (v.Contains(lexeme))
                {
                    internalMatrix[i] = "1";
                }
                else
                {
                    internalMatrix[i] = "0";
                }

                i++;
            }

            matrixLine[index++] = $"{lexeme};{string.Join(',', internalMatrix)}";
        }

        return string.Join('\n', matrixLine);
    }
}