using DistEdu.BooleanSearch.Models;

namespace DistEdu.BooleanSearch.DataSource;

public interface IBookDataSource
{
    /// <summary>
    ///     In real life we will read data piece by piece, but here we can read at all - we store it in RAM anyway.
    /// </summary>
    /// <returns></returns>
    Dictionary<int, Book> GetAllBooks();

    List<int> GetAllIds();
}