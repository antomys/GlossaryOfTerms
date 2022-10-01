using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml;
using FB2Library;

namespace DistEdu.Common;

public sealed class IoExtensions
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
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

    public static string[] GetFileNames(string folderName, string fileExt)
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
}