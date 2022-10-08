using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Xml;
using DistEdu.Common.Models;
using FB2Library;

namespace DistEdu.Common.Extensions;

public static partial class IoExtensions
{
    private const char DotDelimiter = '.';
    private const char PathDelimiter = '/';

    private static readonly JsonSerializerOptions? SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        IncludeFields = true
    };

    private static readonly XmlReaderSettings ReaderSettings = new()
    {
        DtdProcessing = DtdProcessing.Ignore
    };

    private static readonly XmlLoadSettings LoadSettings = new(ReaderSettings);

    public static string[] GetFileNames(string folderName, string fileExtension)
    {
        var projectDirectory =
            $"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName}/{folderName}";

        if (Directory.Exists(projectDirectory) is false) throw new DirectoryNotFoundException(nameof(folderName));

        var fileNames = Directory
            .GetFiles(projectDirectory, GetFileExtension(fileExtension), SearchOption.AllDirectories)
            .ToArray();

        return fileNames;
    }

    public static async Task<T?> ReadTextAsync<T>(string filePath)
    {
        await using var sourceStream =
            new FileStream(
                filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);

        var sb = new StringBuilder();

        var buffer = new byte[0x1000];
        int numRead;
        while ((numRead = await sourceStream.ReadAsync(buffer)) != 0)
        {
            var text = Encoding.UTF8.GetString(buffer, 0, numRead);
            sb.Append(text);
        }

        return JsonSerializer.Deserialize<T>(sb.ToString(), SerializerOptions);
    }

    public static async Task<T?> ReadMsgPackAsync<T>(string filePath)
    {
        await using var openStream = File.OpenRead(filePath);

        return await MessagePack.MessagePackSerializer.DeserializeAsync<T>(openStream);
    }

    public static async Task<Fb2FileWrapper> ReadFb2FileAsync(string filePath, int fileIndex)
    {
        await using var sourceStream = File.OpenRead(filePath);

        var fb2Reader = new FB2Reader();
        try
        {
            // reading
            var file = await fb2Reader.ReadAsync(sourceStream, LoadSettings);

            return new Fb2FileWrapper(fileIndex, file, filePath.GetFileName());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file : {ex.Message}");

            return await Task.FromException<Fb2FileWrapper>(ex);
        }
    }

    public static Task<Fb2FileWrapper[]> ReadFb2FilesAsync(string[] filesPath)
    {
        var tasks = new Task<Fb2FileWrapper>[filesPath.Length];

        for (var fileIndex = 0; fileIndex < filesPath.Length; fileIndex++)
            tasks[fileIndex] = ReadFb2FileAsync(filesPath[fileIndex], fileIndex);

        return Task.WhenAll(tasks);
    }

    public static Dictionary<T2, T1> Transpose<T1, T2, TEnumerable>(this ConcurrentDictionary<T1, TEnumerable> source)
        where TEnumerable : HashSet<T2>
        where T2 : notnull
        where T1 : notnull
    {
        return source
            .SelectMany(keyValuePair => keyValuePair.Value.Select(t2 => new { Key = t2, Value = keyValuePair.Key }))
            .ToDictionary(keyValue => keyValue.Key, keyValue => keyValue.Value);
    }

    private static string GetFileExtension(this string fileExtension)
    {
        return fileExtension[0] is DotDelimiter
            ? fileExtension
            : $"*{DotDelimiter}{fileExtension}";
    }

    private static ReadOnlySpan<char> GetFileName(this string filePath)
    {
        var filePathSpan = filePath.AsSpan();

        return filePathSpan[(filePathSpan.LastIndexOf(PathDelimiter) + 1)..];
    }
}