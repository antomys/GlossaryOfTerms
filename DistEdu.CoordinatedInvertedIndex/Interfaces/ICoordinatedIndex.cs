namespace DistEdu.CoordinatedInvertedIndex.Interfaces;

public interface ICoordinatedIndex
{
    List<CoordIndexToken> Find(string text);

    IEnumerable<KeyValuePair<string, CoordIndexToken>> FindV2(string text);

    IEnumerable<KeyValuePair<string, CoordIndexToken>> All();

    void Add(string term, int id, string fileName, int positionInFile);

    int Size();
}