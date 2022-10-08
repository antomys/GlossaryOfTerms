using System.Diagnostics;
using DistEdu.BooleanSearch.Extensions;
using DistEdu.BooleanSearch.QueryParsing.Tokenize;

namespace DistEdu.BooleanSearch.QueryParsing;

[DebuggerDisplay("{string.IsNullOrEmpty(Term) ? Operation : Term}")]
public sealed class ExpressionTreeNode
{
    public string Operation { get; private init; }

    public string Term { get; private init; }

    public ExpressionTreeNode Child1 { get; set; }

    public ExpressionTreeNode Child2 { get; set; }

    public static ExpressionTreeNode CreateTerm(string term)
    {
        return new ExpressionTreeNode
        {
            Term = term
        };
    }

    public static ExpressionTreeNode CreateNot(ExpressionTreeNode term)
    {
        return new ExpressionTreeNode
        {
            Operation = Token.Not.GetName(),
            Child1 = term
        };
    }

    public static ExpressionTreeNode CreateOr(ExpressionTreeNode child1, ExpressionTreeNode child2)
    {
        return new ExpressionTreeNode
        {
            Operation = Token.Or.GetName(),
            Child1 = child1,
            Child2 = child2
        };
    }

    public static ExpressionTreeNode CreateAnd(ExpressionTreeNode child1, ExpressionTreeNode child2)
    {
        return new ExpressionTreeNode
        {
            Operation = Token.And.GetName(),
            Child1 = child1,
            Child2 = child2
        };
    }

    public static ExpressionTreeNode CreateAllNode()
    {
        return new ExpressionTreeNode
        {
            Operation = Token.All.GetName()
        };
    }

    public static ExpressionTreeNode CreateZeroNode()
    {
        return new ExpressionTreeNode
        {
            Operation = Token.Zero.GetName()
        };
    }
}