namespace DistEdu.Common;

public interface IDataSource<T>
{
    /// <summary>
    ///     In real life we will read data piece by piece, but here we can read at all - we store it in RAM anyway.
    /// </summary>
    /// <returns>
    ///     Dictionary.
    /// </returns>
    Dictionary<int, T> Get();

    List<int> GetKeys();
}

public interface IDataSourceV2<T>
{
    /// <summary>
    ///     In real life we will read data piece by piece, but here we can read at all - we store it in RAM anyway.
    /// </summary>
    /// <returns>
    ///     Dictionary.
    /// </returns>
    Dictionary<int, T> Get();

    Dictionary<int, T>.KeyCollection GetKeys();
}