using MessagePack;

namespace DistEdu.CoordinatedInvertedIndex;

[Serializable]
[MessagePackObject(true)]
public sealed class CoordinatedIndexes
{
    public Dictionary<string, CoordIndexToken[]> Indexes { get; init; }
}