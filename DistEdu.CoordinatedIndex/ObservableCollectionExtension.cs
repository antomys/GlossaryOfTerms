using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace DistEdu.CoordinatedIndex;

public static class ObservableCollectionExtension
{
    static readonly ConditionalWeakTable<object, object> IndexLookup = new();
    public static IQueryable<T> ToIndexedQueryable<T>(this ObservableCollection<T> col) 
    {
        return col.ToIndexedQueryable<T, ObservableCollection<T>>();
    }

    public static IQueryable<T> ToIndexedQueryable<T, TCollection>(this TCollection col) where TCollection : INotifyCollectionChanged, IEnumerable<T> 
    {
        var index = (Linqdex<T>) IndexLookup.GetValue(col, key =>
        {
            var list = new Linqdex<T>();
            list.AddRange(col);
            col.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        list.AddRange(e.NewItems.Cast<T>());
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        list.RemoveRange(e.OldItems.Cast<T>());
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        list.AddRange(e.NewItems.Cast<T>());
                        list.RemoveRange(e.OldItems.Cast<T>());
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        list.Clear();
                        list.AddRange(col);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            };
            return list;
        });
            
        return index.ToIndexedQuery();
    }
}