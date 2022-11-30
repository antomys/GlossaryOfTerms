using MessagePack;

namespace DistEdu.CoordinatedInvertedIndex;

[MessagePackObject(true)]
public sealed class CoordIndexToken
{
    public string FileName { get; init; }
    
    public int InvertedIndex { get; init; }
    
    public bool IsRemoved { get; set; }
    
    public HashSet<int>? Found { get; set; }

    public HashSet<int> PositionInFile { get; init; } = new();

    public HashSet<int> Collection => Found?.Count is 0 or null ? PositionInFile : Found;
}