using DistEdu.CoordinatedInvertedIndex.Interfaces;
using MessagePack;

namespace DistEdu.CoordinatedInvertedIndex;

/// <summary>
///     Serializable - just for memory statistics
///     and for potential saving in file.
/// </summary>
[Serializable]
[MessagePackObject]
public sealed class CoordinatedIndex : Dictionary<string, CoordIndexToken>, ICoordinatedIndex
{
    public CoordinatedIndex()
    {
    }

    public CoordinatedIndex(int count)
        : base(count)
    {
    }

    public List<CoordIndexToken> Find(string text)
    {
        // make a copy for list just in case...
        var result = new List<CoordIndexToken>();

        if (ContainsKey(text))
        {
            result.Add(this[text]);
        }

        return result;
    }

    public IEnumerable<KeyValuePair<string, CoordIndexToken>> FindV2(string text)
    {
        return this.Where(kvp => kvp.Key.Equals(text, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<KeyValuePair<string, CoordIndexToken>> All()
    {
        return this.Select(keyValuePair => keyValuePair);
    }

    public void Add(string term, int id, string fileName, int positionInFile)
    {
        if (TryGetValue(term, out var coordIndexTokens))
        {
            coordIndexTokens.PositionInFile.Add(positionInFile);
            this[term] = coordIndexTokens;
        }
        else
        {
            this[term] = new CoordIndexToken
            {
                FileName = fileName,
                InvertedIndex = id,
                PositionInFile = new HashSet<int>
                {
                    positionInFile,
                }
            };
        }
    }

    public int Size()
    {
        return Keys.Count;
    }
}