using DistEdu.BooleanSearch.DataSource;
using DistEdu.BooleanSearch.Indexing;
using DistEdu.BooleanSearch.QueryParsing;

namespace DistEdu.BooleanSearch.Searching;

public sealed class EtSearcher
{
    private readonly IBookDataSource _dataSource;
    private readonly IIndex _index;

    public EtSearcher(IIndex index, IBookDataSource dataSource)
    {
        _index = index;
        _dataSource = dataSource;
    }

    public List<int> Search(string query)
    {
        var parser = new EtQueryParser();
        var tree = parser.Parse(query);

        return SearchInTree(tree);
    }

    private List<int> SearchInTree(ExpressionTreeNode node, bool notCompareWithAll = false)
    {
        if (node is null)
        {
            throw new Exception("Invalid expression tree");
        }

        if (string.IsNullOrEmpty(node.Term) is false)
        {
            return _index.Find(node.Term);
        }

        return node.Operation switch
        {
            "AND" => node.Child2.Operation is "NOT"
                ? SearchInTree(node.Child1).Except(SearchInTree(node.Child2, true)).ToList()
                : SearchInTree(node.Child1).Intersect(SearchInTree(node.Child2)).ToList(),
            "OR" => SearchInTree(node.Child1).Concat(SearchInTree(node.Child2)).ToList(),
            "NOT" => notCompareWithAll
                ? SearchInTree(node.Child1).ToList()
                : _dataSource.GetAllIds().Except(SearchInTree(node.Child1)).ToList(),
            "ALL" => _dataSource.GetAllIds(),
            "ZERO" => new List<int>(),
            _ => throw new Exception("Invalid expression tree")
        };
    }
}