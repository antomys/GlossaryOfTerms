using System.Text;
using DistEdu.Dictionary;

namespace DistEdu.Host;

public sealed class Reader
{
    private static readonly string OutputDirectory =
        $"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName}/Output";

    static Reader()
    {
        if (Directory.Exists(OutputDirectory) is false) Directory.CreateDirectory(OutputDirectory);
    }

    // public async Task ProcessCsvValues(string folderName)
    // {
    //     var filesInFolder = IoExtensions.GetFileNames(folderName, "fb2");
    //
    //     var fb2Files = filesInFolder.Select((file, index) => IoExtensions.ReadFb2FilesV3Async(file, index)
    //         .ContinueWith(task => new Fb2Processor(task.Result).ProcessFb2Async(), TaskContinuationOptions.AttachedToParent))
    //         .ToArray();
    //     
    //     await Task.WhenAll(fb2Files);
    //
    //     await WriteCustomCsvFileAsync(fb2Files.SelectMany(x => x.Result));
    // }
    //
    // public async Task ProcessMatrixValues(string folderName)
    // {
    //     var filesInFolder = IoExtensions.GetFileNames(folderName, "fb2").ToArray();
    //
    //     var fb2Files = filesInFolder.Select((file, index) => IoExtensions.ReadFb2FilesV3Async(file, index)
    //             .ContinueWith(task => new Fb2Processor(task.Result).ProcessFb2WithMatrixAsync(), TaskContinuationOptions.AttachedToParent))
    //         .ToArray();
    //     
    //     await Task.WhenAll(fb2Files);
    //
    //     var tuples = fb2Files
    //         .Select(task => (task.Result.Keys, task.Result))
    //         .ToArray();
    //
    //     var keys = tuples
    //         .SelectMany(valueTuple => valueTuple.Keys)
    //         .ToArray();
    //
    //     var dictionary = tuples
    //         .SelectMany(valueTuple=> valueTuple.Result.Transpose<string, string, HashSet<string>>())
    //         .GroupBy(keyValuePair => keyValuePair.Key)
    //         .ToDictionary(valuePairs => valuePairs.Key, x => x.Select(keyValuePair=> keyValuePair.Value));
    //     
    //     await WriteMatrixToFileV2Async(keys, dictionary);
    // }

    private async Task WriteCustomCsvFileAsync(IEnumerable<FileToken> tokens)
    {
        const string fileNameDir = "Custom_csv.csv";

        var file = Path.Combine(OutputDirectory, fileNameDir);

        Console.WriteLine($"Async Write {fileNameDir} File has started.");

        if (File.Exists(file)) File.Delete(file);

        var index = 0;
        await using (var outputFile = new StreamWriter(file, false, Encoding.UTF8))
        {
            var headers = FileToken.GetHeaders();
            await outputFile.WriteLineAsync(headers);

            foreach (var fileToken in tokens)
            {
                if (fileToken is null || string.IsNullOrWhiteSpace(fileToken.Token)) continue;

                fileToken.Id = index;
                Interlocked.Increment(ref index);

                await outputFile.WriteLineAsync(fileToken.ToString());
            }
        }

        Console.WriteLine($"Async Write {fileNameDir} File has completed.");
    }

    private static async Task WriteMatrixToFileAsync(Dictionary<string, IEnumerable<string>> lexemes)
    {
        const string fileNameDir = "Custom_Matrix.csv";

        var file = Path.Combine(OutputDirectory, fileNameDir);

        Console.WriteLine($"Matrix async Write {fileNameDir} File has started.");

        if (File.Exists(file)) File.Delete(file);

        var index = 0;
        await using (var outputFile = new StreamWriter(file, false, Encoding.UTF8))
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

        if (File.Exists(file)) File.Delete(file);

        var index = 0;
        await using (var outputFile = new StreamWriter(file, false, Encoding.UTF8))
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

        for (var i = 0; i < keys.Count; i++)
            if (fileNames.Contains(keys[i]))
                array[i] = 1;
            else
                array[i] = 0;

        return GetString(array);
    }

    private static string GetString<T>(IEnumerable<T> values)
    {
        var sb = new StringBuilder();
        sb.AppendJoin(';', values);

        return sb.ToString();
    }
}