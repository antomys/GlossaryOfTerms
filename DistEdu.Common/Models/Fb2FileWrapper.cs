using FB2Library;

namespace DistEdu.Common.Models;

public sealed class Fb2FileWrapper
{
    public Fb2FileWrapper(int fileIndex, FB2File file, string fileName)
    {
        FileIndex = fileIndex;
        File = file;
        FileName = fileName;
    }

    public Fb2FileWrapper(int fileIndex, FB2File file, ReadOnlySpan<char> fileName)
    {
        FileIndex = fileIndex;
        File = file;
        FileName = fileName.ToString();
    }

    public string FileName { get; }

    public FB2File File { get; }

    public int FileIndex { get; }
}