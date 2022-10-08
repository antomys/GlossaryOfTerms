namespace DistEdu.Index.Interfaces;

public interface IIndex
{
    List<string> Find(string text);

    IEnumerable<KeyValuePair<string, HashSet<string>>> FindV2(string text);

    IEnumerable<KeyValuePair<string, HashSet<string>>> All();

    void Add(string term, string id);

    int Size();
}