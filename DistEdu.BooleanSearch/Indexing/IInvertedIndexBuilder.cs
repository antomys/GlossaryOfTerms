using DistEdu.BooleanSearch.DataSource;

namespace DistEdu.BooleanSearch.Indexing;

public interface IInvertedIndexBuilder
{
    IIndex BuildIndex(IBookDataSource dataSource);
}