using System.Text;
using System.Text.Json;

namespace Common;

public static partial class IoExtensions
{
    public static async Task WriteJsonFileAsync<T>(this T value, string outputDirectory, string? fileName = null)
    {
        fileName ??= Guid.NewGuid().ToString();
        
        var fileNameDir = $"{fileName}.json";

        var file = Path.Combine(outputDirectory, fileNameDir);
        
        Console.WriteLine($"Async Write of File {fileNameDir} has started.");
       
        if (File.Exists(file))
        {
            File.Delete(file);
        }

        var serializedContent = JsonSerializer.Serialize(value, SerializerOptions);
        
        await using(var outputFile = new StreamWriter(file, append: false, encoding: Encoding.UTF8))
        {
            await outputFile.WriteAsync(serializedContent);
        }
        
        Console.WriteLine($"Async Write of File {fileNameDir} has completed.");
    }
    
    public static async Task WriteMsgPackFileAsync<T>(this T value, string outputDirectory, string? fileName = null)
    {
        fileName ??= Guid.NewGuid().ToString();
        
        var fileNameDir = $"{fileName}.msgpack";
        
        var file = Path.Combine(outputDirectory, fileNameDir);
        
        Console.WriteLine($"Async Write of File {fileNameDir} has started.");
       
        if (File.Exists(file))
        {
            File.Delete(file);
        }

        var serializedContent = MessagePack.MessagePackSerializer.Serialize(value)!;

        await using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
        {
            await fs.WriteAsync(serializedContent);
            
        }
        
        Console.WriteLine($"Async Write of File {fileNameDir} has completed.");
    }
}