namespace DistEdu.BooleanSearch.Indexing;

public interface IIndex
{
    List<int> Find(string text);

    void Add(string term, int id);

    int Size();
}