using MessagePack;

namespace DistEdu.CoordinatedInvertedIndex;

[MessagePackObject(true)]
public sealed class CoordIndexToken
{
    public string FileName { get; init; }
    
    public int InvertedIndex { get; init; }

    public HashSet<int> PositionInFile { get; set; } = new();
}