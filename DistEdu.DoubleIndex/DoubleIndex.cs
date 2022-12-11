using System.Collections.Concurrent;

namespace DistEdu.DoubleIndex;

public sealed class DoubleIndex : ConcurrentDictionary<string, HashSet<string>>
{
    
}