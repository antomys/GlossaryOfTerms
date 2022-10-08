using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using DistEdu.Common;
using DistEdu.Common.Extensions;

namespace DistEdu.Matrix;

public sealed class MatrixService : ServiceBase
{
    private static readonly
        Expression<Func<ConcurrentDictionary<string, HashSet<string>>[], Dictionary<string, IEnumerable<string>>>>
        MatrixExpression
            = concurrentDictionaries
                => concurrentDictionaries
                    .SelectMany(dict => dict.Transpose<string, string, HashSet<string>>())
                    .GroupBy(keyValuePair => keyValuePair.Key)
                    .ToDictionary(grouping => grouping.Key, valuePairs => valuePairs.Select(y => y.Value));

    private static readonly
        Func<ConcurrentDictionary<string, HashSet<string>>[], Dictionary<string, IEnumerable<string>>>
        ProcessDictionaries = MatrixExpression.Compile();

    private Dictionary<string, IEnumerable<string>> _adjacencyMatrix;

    private string[] _fileNames;

    public MatrixService(string folderName, string filesExtension)
        : base(folderName, filesExtension, nameof(MatrixService))
    {
        MatrixExpression.Compile();

        _fileNames = Array.Empty<string>();
        _adjacencyMatrix = new Dictionary<string, IEnumerable<string>>();
    }

    public async Task GetAdjacencyMatrix()
    {
        var sw = new Stopwatch();

        var filesInFolder = GetFiles();

        if (filesInFolder.Length is 0) return;

        sw.Start();

        var fb2Files = filesInFolder.Select((filePath, index) => IoExtensions.ReadFb2FileAsync(filePath, index)
            .ContinueWith(task => new AdjacencyMatrixFb2Processor(task.Result).ProcessFb2WithMatrixAsync(),
                TaskContinuationOptions.AttachedToParent));

        var dictionaries = await Task.WhenAll(fb2Files);

        _fileNames = dictionaries
            .Select(task => (task.Keys, task))
            .SelectMany(valueTuple => valueTuple.Keys)
            .ToArray();

        _adjacencyMatrix = ProcessDictionaries(dictionaries);

        sw.Stop();

        Console.WriteLine($"Elapsed time of processing : {sw.ElapsedMilliseconds} ms");
    }

    public async Task WriteAdjacencyMatrixAsync()
    {
        if (_adjacencyMatrix.Count is 0)
        {
            Console.WriteLine("Unable to write, matrix is empty");

            return;
        }

        const string fileNameDir = "AdjacencyMatrix.csv";

        var file = Path.Combine(OutputPath, fileNameDir);

        Console.WriteLine($"Matrix async Write {fileNameDir} File has started.");

        if (File.Exists(file)) File.Delete(file);

        await using (var outputFile = new StreamWriter(file, false, Encoding.UTF8))
        {
            await JsonSerializer.SerializeAsync(outputFile.BaseStream, _adjacencyMatrix);
        }

        Console.WriteLine($"Matrix async Write {fileNameDir} File has completed.");
    }

    public async Task WritePrettyAdjacencyMatrixAsync()
    {
        if (_adjacencyMatrix.Count is 0)
        {
            Console.WriteLine("Unable to write, matrix is empty");

            return;
        }

        const string fileNameDir = "AdjacencyMatrix_Pretty.csv";

        var file = Path.Combine(OutputPath, fileNameDir);

        Console.WriteLine($"Matrix async Write {fileNameDir} File has started.");

        if (File.Exists(file)) File.Delete(file);

        var index = 0;
        await using (var outputFile = new StreamWriter(file, false, Encoding.UTF8))
        {
            var headers = $"Id;Lexeme;{_fileNames.GetString()}";
            await outputFile.WriteLineAsync(headers);

            foreach (var (lexeme, fileNames) in _adjacencyMatrix)
            {
                Interlocked.Increment(ref index);

                var writeString = $"{index};{lexeme};{GetMatrix(_fileNames, fileNames.ToArray())}";

                await outputFile.WriteLineAsync(writeString);
            }
        }

        Console.WriteLine($"Matrix async Write {fileNameDir} File has completed.");
    }

    private static string GetMatrix(IReadOnlyList<string> keys, string[] fileNames)
    {
        var array = new int[keys.Count];

        for (var i = 0; i < keys.Count; i++)
            if (fileNames.Contains(keys[i]))
                array[i] = 1;
            else
                array[i] = 0;

        return array.GetString();
    }
}