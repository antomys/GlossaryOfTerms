using DistEdu.Index.Interfaces;
using MessagePack;

namespace DistEdu.Index;

/// <summary>
///     Serializable - just for memory statistics
///     and for potential saving in file.
/// </summary>
[Serializable]
[MessagePackObject]
public sealed class InvertedIndex : Dictionary<string, HashSet<string>>, IIndex
{
    public InvertedIndex()
    {
    }

    public InvertedIndex(int count)
        : base(count)
    {
    }

    public List<string> Find(string text)
    {
        // make a copy for list just in case...
        var result = new List<string>();

        if (ContainsKey(text))
        {
            result.AddRange(this[text]);
        }

        return result;
    }

    public IEnumerable<KeyValuePair<string, HashSet<string>>> FindV2(string text)
    {
        return this.Where(kvp => kvp.Key.Equals(text, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<KeyValuePair<string, HashSet<string>>> All()
    {
        return this.Select(keyValuePair => keyValuePair);
    }

    public void Add(string term, string id)
    {
        if (ContainsKey(term))
        {
            if (!this[term].Contains(id)) this[term].Add(id);
        }
        else
        {
            this[term] = new HashSet<string>
            {
                id
            };
        }
    }

    public int Size()
    {
        return Keys.Count;
    }
}