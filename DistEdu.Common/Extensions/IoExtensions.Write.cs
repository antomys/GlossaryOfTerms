using System.Text;
using System.Text.Json;
using MessagePack;

namespace DistEdu.Common.Extensions;

public static partial class IoExtensions
{
    public static async Task WriteJsonFileAsync<T>(this T value, string outputDirectory, string? fileName = null)
    {
        fileName ??= Guid.NewGuid().ToString();

        var fileNameDir = $"{fileName}.json";

        var file = Path.Combine(outputDirectory, fileNameDir);

        Console.WriteLine($"Async Write of File {fileNameDir} has started.");

        if (File.Exists(file)) File.Delete(file);

        var serializedContent = JsonSerializer.Serialize(value, SerializerOptions);

        await using (var outputFile = new StreamWriter(file, false, Encoding.UTF8))
        {
            await outputFile.WriteAsync(serializedContent);
        }

        Console.WriteLine($"Async Write of File {fileNameDir} has completed.");
    }
    
    public static async Task WriteJsonFileV2Async<T>(this T value, string outputDirectory, string? fileName = null)
    {
        fileName ??= Guid.NewGuid().ToString();

        var fileNameDir = $"{fileName}.json";

        var file = Path.Combine(outputDirectory, fileNameDir);

        Console.WriteLine($"Async Write of File {fileNameDir} has started.");

        if (File.Exists(file)) File.Delete(file);

        await using var createStream = File.Create(file);
        await JsonSerializer.SerializeAsync(createStream, value, SerializerOptions);
        await createStream.DisposeAsync();

        Console.WriteLine($"Async Write of File {fileNameDir} has completed.");
    }

    public static async Task WriteMsgPackFileAsync<T>(this T value, string outputDirectory, string? fileName = null)
    {
        fileName ??= Guid.NewGuid().ToString();

        var fileNameDir = $"{fileName}.msgpack";

        var file = Path.Combine(outputDirectory, fileNameDir);

        Console.WriteLine($"Async Write of File {fileNameDir} has started.");

        if (File.Exists(file)) File.Delete(file);

        var serializedContent = MessagePackSerializer.Serialize(value)!;

        await using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
        {
            await fs.WriteAsync(serializedContent);
        }

        Console.WriteLine($"Async Write of File {fileNameDir} has completed.");
    }
}