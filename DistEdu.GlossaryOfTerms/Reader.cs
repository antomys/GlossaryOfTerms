using System.Collections.Concurrent;
using System.Text;
using Common;

namespace DistEdu.GlossaryOfTerms;

public sealed class Reader
{
    private static readonly string OutputDirectory = $"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName}/Output";

    private static readonly ConcurrentDictionary<string, int> GlossaryOfTerms = new();

    static Reader()
    {
        if (Directory.Exists(OutputDirectory) is false)
        {
            Directory.CreateDirectory(OutputDirectory);
        }
    }

    public static async Task ProcessValues(string folderName)
    {
        var filesInFolder = IoExtensions.GetFileNames(folderName, "fb2");

        var fb2Files = filesInFolder.Select(file => IoExtensions.ReadFb2FilesV2Async(file)
            .ContinueWith(task => Fb2Processor.ProcessFb2(task.Result, GlossaryOfTerms), TaskContinuationOptions.AttachedToParent));
        
        await Task.WhenAll(fb2Files);
    }
    
    public static async Task ProcessCsvValues(string folderName)
    {
        var filesInFolder = IoExtensions.GetFileNames(folderName, "fb2");

        var fb2Files = filesInFolder.Select((file, index) => IoExtensions.ReadFb2FilesV3Async(file, index)
            .ContinueWith(task => new Fb2Processor(task.Result).ProcessFb2Async(), TaskContinuationOptions.AttachedToParent))
            .ToArray();
        
        await Task.WhenAll(fb2Files);

        await WriteCustomCsvFileAsync(fb2Files.SelectMany(x => x.Result));
    }
    
    public static async Task ProcessMatrixValues(string folderName)
    {
        var filesInFolder = IoExtensions.GetFileNames(folderName, "fb2").ToArray();

        var fb2Files = filesInFolder.Select((file, index) => IoExtensions.ReadFb2FilesV3Async(file, index)
                .ContinueWith(task => new Fb2Processor(task.Result).ProcessFb2WithMatrixAsync(), TaskContinuationOptions.AttachedToParent))
            .ToArray();
        
        await Task.WhenAll(fb2Files);

        var tuples = fb2Files
            .Select(task => (task.Result.Keys, task.Result))
            .ToArray();

        var keys = tuples
            .SelectMany(valueTuple => valueTuple.Keys)
            .ToArray();

        var dictionary = tuples
            .SelectMany(valueTuple=> valueTuple.Result.ReverseDictionary<string, string, HashSet<string>>())
            .GroupBy(keyValuePair => keyValuePair.Key)
            .ToDictionary(valuePairs => valuePairs.Key, x => x.Select(keyValuePair=> keyValuePair.Value));
        
        await WriteMatrixToFileV2Async(keys, dictionary);
    }

    public static async Task WriteCustomFileAsync()
    {
        const string fileNameDir = "Custom.bin";
        
        var file = Path.Combine(OutputDirectory, fileNameDir);

        Console.WriteLine("Async Write File has started.");

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        await using(var outputFile = new StreamWriter(file, append: false, encoding: Encoding.UTF8))
        {
            foreach (var item in GlossaryOfTerms)
            {
                await outputFile.WriteLineAsync(item.ToString());
            }
        }
        Console.WriteLine("Async Write File has completed.");
    }
    
    private static async Task WriteCustomCsvFileAsync(IEnumerable<FileToken> tokens)
    {
        const string fileNameDir = "Custom_csv.csv";
        
        var file = Path.Combine(OutputDirectory, fileNameDir);

        Console.WriteLine($"Async Write {fileNameDir} File has started.");

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        var index = 0;
        await using(var outputFile = new StreamWriter(file, append: false, encoding: Encoding.UTF8))
        {
            var headers = FileToken.GetHeaders();
            await outputFile.WriteLineAsync(headers);
            
            foreach (var fileToken in tokens)
            {
                if (fileToken is null || string.IsNullOrWhiteSpace(fileToken.Token))
                {
                    continue;
                }
                
                fileToken.Id = index;
                Interlocked.Increment(ref index);
                
                await outputFile.WriteLineAsync(fileToken.ToString());
            }
        }
        Console.WriteLine($"Async Write {fileNameDir} File has completed.");
    }
    
    private static async Task WriteMatrixToFileAsync(Dictionary<string,IEnumerable<string>> lexemes)
    {
        const string fileNameDir = "Custom_Matrix.csv";
        
        var file = Path.Combine(OutputDirectory, fileNameDir);

        Console.WriteLine($"Matrix async Write {fileNameDir} File has started.");

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        var index = 0;
        await using(var outputFile = new StreamWriter(file, append: false, encoding: Encoding.UTF8))
        {
            foreach (var (lexeme, fileNames) in lexemes)
            {
                Interlocked.Increment(ref index);
                
                var writeString = $"{index};{lexeme};{GetString(fileNames)}";
                
                await outputFile.WriteLineAsync(writeString);
            }
        }
        
        Console.WriteLine($"Matrix async Write {fileNameDir} File has completed.");
    }
    
    private static async Task WriteMatrixToFileV2Async(
        IReadOnlyList<string> keys,
        Dictionary<string, IEnumerable<string>> lexemes)
    {
        const string fileNameDir = "Custom_Matrix.csv";
        
        var file = Path.Combine(OutputDirectory, fileNameDir);

        Console.WriteLine($"Matrix async Write {fileNameDir} File has started.");

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        var index = 0;
        await using(var outputFile = new StreamWriter(file, append: false, encoding: Encoding.UTF8))
        {
            var headers = $"Id;Lexeme;{GetString(keys)}";
            await outputFile.WriteLineAsync(headers);
            
            foreach (var (lexeme, fileNames) in lexemes)
            {
                Interlocked.Increment(ref index);
                
                var writeString = $"{index};{lexeme};{GetMatrix(keys, fileNames.ToArray())}";
                
                await outputFile.WriteLineAsync(writeString);
            }
        }
        
        Console.WriteLine($"Matrix async Write {fileNameDir} File has completed.");
    }

    private static string GetMatrix(IReadOnlyList<string> keys, string[] fileNames)
    {
        var array = new int[keys.Count];

        for(var i = 0; i < keys.Count; i++)
        {
            if (fileNames.Contains(keys[i]))
            {
                array[i] = 1;
            }
            else
            {
                array[i] = 0;
            }
        }

        return GetString(array);
    }

    private static string GetString<T>(IEnumerable<T> values)
    {
        var sb = new StringBuilder();
        sb.AppendJoin(';', values);

        return sb.ToString();
    }

    public static Task WriteJsonFileAsync() =>
        GlossaryOfTerms.WriteJsonFileAsync(OutputDirectory, "GlossaryJson");
    
    public static Task WriteMsgPackFileAsync() =>
        GlossaryOfTerms.WriteMsgPackFileAsync(OutputDirectory, "GlossaryMsgPack");
}