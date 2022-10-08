using DistEdu.BooleanSearch.QueryParsing;
using DistEdu.Index.Interfaces;

namespace DistEdu.BooleanSearch.Searching;

public sealed class EtSearcher
{
    private readonly string[] _fileNames;
    private readonly IIndex _index;

    public EtSearcher(IIndex index, string[] fileNames)
    {
        _index = index;
        _fileNames = fileNames;
    }

    public IEnumerable<string> Search(string query)
    {
        var parser = EtQueryParser.CreateInstance(query);
        var tree = parser.Parse();

        return SearchInTree(tree);
    }

    public KeyValuePair<string, HashSet<string>>[]? SearchV2(string query)
    {
        var parser = EtQueryParser.CreateInstance(query);
        var tree = parser.Parse();

        return SearchInTreeV2(tree)?.ToArray();
    }

    private IEnumerable<string> SearchInTree(ExpressionTreeNode node, bool notCompareWithAll = false)
    {
        if (node is null) throw new Exception("Invalid expression tree");

        if (string.IsNullOrEmpty(node.Term) is false) return _index.Find(node.Term);

        return node.Operation switch
        {
            "AND" => node.Child2.Operation is "NOT"
                ? SearchInTree(node.Child1).Except(SearchInTree(node.Child2, true))
                : SearchInTree(node.Child1).Intersect(SearchInTree(node.Child2)),
            "OR" => SearchInTree(node.Child1).Concat(SearchInTree(node.Child2)),
            "NOT" => notCompareWithAll
                ? SearchInTree(node.Child1).ToList()
                : _fileNames.Except(SearchInTree(node.Child1)),
            "ALL" => _fileNames,
            "ZERO" => Array.Empty<string>(),
            _ => throw new Exception("Invalid expression tree")
        };
    }

    private KeyValuePair<string, HashSet<string>>[] SearchInTreeV2(ExpressionTreeNode node,
        bool notCompareWithAll = false)
    {
        if (node is null) throw new Exception("Invalid expression tree");

        if (string.IsNullOrEmpty(node.Term) is false) return _index.FindV2(node.Term)?.ToArray();

        return node.Operation switch
        {
            "AND" => node.Child2.Operation is "NOT"
                ? SearchInTreeV2(node.Child1)?.Except(SearchInTreeV2(node.Child2, true)).ToArray()
                : SearchInTreeV2(node.Child1)?.Intersect(SearchInTreeV2(node.Child2)).ToArray(),
            "OR" => SearchInTreeV2(node.Child1)?.Concat(SearchInTreeV2(node.Child2)).ToArray(),
            "NOT" => notCompareWithAll
                ? SearchInTreeV2(node.Child1)?.ToArray()
                : _index.All()?.Except(SearchInTreeV2(node.Child1)).ToArray(),
            "ALL" => _index.All()?.ToArray(),
            "ZERO" => null,
            _ => throw new Exception("Invalid expression tree")
        };
    }
}