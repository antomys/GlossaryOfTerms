using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using DistEdu.Common;
using DistEdu.Common.Extensions;

namespace DistEdu.Dictionary;

public sealed class DictionaryService : ServiceBase
{
    private readonly ConcurrentDictionary<string, int> _glossaryOfTerms;

    public DictionaryService(string folderName, string filesExtension)
        : base(folderName, filesExtension, nameof(DictionaryService))
    {
        _glossaryOfTerms = new ConcurrentDictionary<string, int>();
    }

    public async Task ProcessAsync()
    {
        var sw = new Stopwatch();

        var filesInFolder = GetFiles();

        if (filesInFolder.Length is 0) return;

        sw.Start();
        var fb2Files = filesInFolder.Select((file, index) => ProcessInternal(file, index));

        await Task.WhenAll(fb2Files);

        sw.Stop();

        Console.WriteLine($"Elapsed time of processing : {sw.ElapsedMilliseconds} ms");
    }

    private Task ProcessInternal(string fileName, int index)
    {
        return IoExtensions
            .ReadFb2FileAsync(fileName, index)
            .ContinueWith(task => new DictionaryFb2Processor(task.Result, _glossaryOfTerms).ProcessFb2(),
                TaskContinuationOptions.AttachedToParent);
    }

    public void GetStatistics()
    {
        Console.WriteLine();
        Console.WriteLine("\t\t~~~~ STATISTICS: ~~~~");
        Console.WriteLine($"> Total unique lexemes: {_glossaryOfTerms.Count} lexemes");
        Console.WriteLine($"> Total all lexemes count: {_glossaryOfTerms.Values.Sum()} lexemes");

        Console.WriteLine("> Top 10 most popular lexemes: ");

        var top10Lexemes = _glossaryOfTerms
            .OrderByDescending(kvp => kvp.Value)
            .Take(10);
        var index = 1;

        foreach (var kvp in top10Lexemes) Console.WriteLine($"\t> {index++}. Lexeme: {kvp.Key}; Times: {kvp.Value};");

        var inputFileNames = IoExtensions.GetFileNames(FolderName, FilesExtension);
        Console.WriteLine("> List of processed files and their sizes:");

        if (inputFileNames.Length is 0) return;

        PrintFilesInfo(inputFileNames);

        var outputFileNames = Directory.GetFiles(OutputPath);

        if (outputFileNames.Length is 0) return;

        Console.WriteLine("> List of saved files and their sizes:");
        PrintFilesInfo(outputFileNames);
    }

    public async Task WriteFileAsync()
    {
        const string fileNameDir = $"{nameof(_glossaryOfTerms)}.bin";

        var file = Path.Combine(OutputPath, fileNameDir);

        Console.WriteLine();
        Console.WriteLine($"Async Write file {fileNameDir} has started.");

        if (File.Exists(file)) File.Delete(file);

        await using (var outputFile = new StreamWriter(file, false, Encoding.UTF8))
        {
            foreach (var item in _glossaryOfTerms) await outputFile.WriteLineAsync(item.ToString());
        }

        Console.WriteLine($"Async Write File {fileNameDir} has completed.");
    }

    public Task WriteJsonFileAsync()
    {
        return _glossaryOfTerms.WriteJsonFileAsync(OutputPath, nameof(_glossaryOfTerms));
    }

    public Task WriteMsgPackFileAsync()
    {
        return _glossaryOfTerms.WriteMsgPackFileAsync(OutputPath, nameof(_glossaryOfTerms));
    }
}