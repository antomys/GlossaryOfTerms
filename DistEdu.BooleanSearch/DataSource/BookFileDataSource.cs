using DistEdu.BooleanSearch.Models;

namespace DistEdu.BooleanSearch.DataSource;

public class BookFileDataSource : IBookDataSource
{
    private Dictionary<int, Book> _notebooks;

    public BookFileDataSource(string filename)
    {
        Load(filename);
    }

    public List<int> GetAllIds()
    {
        return _notebooks.Keys.ToList();
    }

    public Dictionary<int, Book> GetAllBooks()
    {
        return _notebooks;
    }

    private void Load(string filename)
    {
        _notebooks = new Dictionary<int, Book>();

        filename =
            $"{Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName}/{filename}";
        
        foreach (var line in File.ReadLines(filename))
        {
            var strings = line.Split(';');
            if (strings.Length == 7 && int.TryParse(strings[0], out var id))
                _notebooks.Add(id,
                    new Book(strings[0], strings[1], strings[2], strings[3], strings[4], strings[5], strings[6]));
        }
    }
}