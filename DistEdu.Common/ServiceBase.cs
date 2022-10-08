using DistEdu.Common.Extensions;
using DistEdu.Common.Models;

namespace DistEdu.Common;

public abstract class ServiceBase
{
    protected readonly string FilesExtension;
    protected readonly string FolderName;
    protected readonly string OutputPath;

    protected ServiceBase(string folderName, string filesExtension, string serviceName)
    {
        OutputPath =
            $"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName}/Output/{serviceName}";
        FilesExtension = filesExtension;
        FolderName = folderName;

        Directory.CreateDirectory(OutputPath);
        Directory.CreateDirectory(
            $"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName}/{folderName}");
    }

    protected async Task<Fb2FileWrapper[]> GetFilesAsync()
    {
        var filesInFolder = IoExtensions.GetFileNames(FolderName, FilesExtension);

        if (FilesExtension != "fb2")
            //  Currently throwing exception as not supporting other extensions.
            throw new NotSupportedException();

        var files = await IoExtensions.ReadFb2FilesAsync(filesInFolder);

        return files;
    }

    protected string[] GetFiles()
    {
        var filesInFolder = IoExtensions.GetFileNames(FolderName, FilesExtension);

        if (filesInFolder.Length is 0)
        {
            Console.WriteLine("~~~ There are no files to process, returning... ~~~");

            return Array.Empty<string>();
        }

        return filesInFolder;
    }

    protected static void PrintFilesInfo(IEnumerable<string> fileNames)
    {
        foreach (var filePath in fileNames)
        {
            var fileInfo = new FileInfo(filePath);

            Console.WriteLine($"\t> Name: {fileInfo.Name}; Size: {FileSizeFormatter.FormatSize(fileInfo.Length)};");
        }
    }
}