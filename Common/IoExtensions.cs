using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml;
using FB2Library;

namespace Common;

public static partial class IoExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        IncludeFields = true,
    };
    
    private static readonly XmlReaderSettings ReaderSettings = new()
    {
        DtdProcessing = DtdProcessing.Ignore
    };
    
    private static readonly XmlLoadSettings LoadSettings = new(ReaderSettings);

    public static IEnumerable<string> GetFileNames(string folderName, string fileExt)
    {
        var projectDirectory = $"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName}/{folderName}";

        if (Directory.Exists(projectDirectory) is false)
        {
            throw new DirectoryNotFoundException(nameof(folderName));
        }

        fileExt = $".{fileExt}";
        return Directory.GetFiles(projectDirectory).Where(file => file.EndsWith(fileExt)).ToArray();
    }
    
    public static async Task<T> ReadTextAsync<T>(string filePath)
    {
        await using var sourceStream =
            new FileStream(
                filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true);

        var sb = new StringBuilder();

        var buffer = new byte[0x1000];
        int numRead;
        while ((numRead = await sourceStream.ReadAsync(buffer)) != 0)
        {
            var text = Encoding.UTF8.GetString(buffer, 0, numRead);
            sb.Append(text);
        }

        return JsonSerializer.Deserialize<T>(sb.ToString(), SerializerOptions)!;
    }
    
    public static async Task<FB2File> ReadFb2FilesAsync(string filePath)
    {
        using var sourceStream = File.OpenText(filePath);

        var sb = new StringBuilder();
        while (!sourceStream.EndOfStream)
        {
            var text = await sourceStream.ReadLineAsync();
            sb.AppendLine(text);
        }
        
        return await new FB2Reader().ReadAsync(sb.ToString());
    }
    
    public static async Task<FB2File> ReadFb2FilesV2Async(string filePath)
    {
        await using var sourceStream = File.OpenRead(filePath);

        var fb2Reader = new FB2Reader();
        try
        {
            // reading
            var file = await fb2Reader.ReadAsync(sourceStream, LoadSettings);

            return file;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading file : {ex.Message}");
           
            return await Task.FromException<FB2File>(ex);
        }
    }
    
    public static async Task<Fb2FileWrapper> ReadFb2FilesV3Async(string filePath, int fileIndex)
    {
        await using var sourceStream = File.OpenRead(filePath);

        var fb2Reader = new FB2Reader();
        try
        {
            // reading
            var file = await fb2Reader.ReadAsync(sourceStream, LoadSettings);

            return new Fb2FileWrapper(fileIndex, file, filePath.Split('/')[^1]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading file : {ex.Message}");
           
            return await Task.FromException<Fb2FileWrapper>(ex);
        }
    }
    
    public static Dictionary<T2, T1> ReverseDictionary<T1, T2, TEnumerable>(this ConcurrentDictionary<T1, TEnumerable> source) where TEnumerable : IEnumerable<T2> 
        where T2 : notnull 
        where T1 : notnull
    {
        return source
            .SelectMany(e => e.Value.Select(s => new { Key = s, Value = e.Key }))
            .ToDictionary(x => x.Key, x => x.Value);
    }
}