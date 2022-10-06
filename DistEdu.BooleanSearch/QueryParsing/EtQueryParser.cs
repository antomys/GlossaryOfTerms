﻿using DistEdu.BooleanSearch.QueryParsing.Tokenize;

namespace DistEdu.BooleanSearch.QueryParsing;

public class EtQueryParser
{
    /// <summary>
    ///     Functions to optimize expression query for faster searcher.
    ///     Maybe should be separated, but let's stay here for now.
    /// </summary>
    private readonly List<OptimizeAction> _optimizeActions =
        new()
        {
            TwoNotInAndRule,
            ReplaceFirstNotInAndRule,
            TwoNotInOrRule,
            HandleAllNode,
            HandleZeroNode
        };

    private TokenReader _tokenReader;

    public ExpressionTreeNode Parse(string query)
    {
        _tokenReader = new TokenReader(query);

        _tokenReader.NextToken();

        var result = ParseOr();

        if (_tokenReader.Token != Token.EOL) throw new Exception("Invalid search string format");

        result = OptimizeTree(result);

        return result;
    }

    private ExpressionTreeNode ParseOr()
    {
        var node = ParseAnd();

        if (_tokenReader.Token != Token.Or) return node;

        _tokenReader.NextToken();

        var root = ExpressionTreeNode.CreateOr(node, ParseAnd());
        node = root;

        while (true)
        {
            if (_tokenReader.Token != Token.Or) return root;

            _tokenReader.NextToken();

            var child1 = node.Child2;

            node.Child2 = ExpressionTreeNode.CreateOr(child1, ParseAnd());

            node = node.Child2;
        }
    }

    private ExpressionTreeNode ParseAnd()
    {
        var node = ParseNot();

        _tokenReader.NextToken();

        if (_tokenReader.Token != Token.And) return node;

        _tokenReader.NextToken();

        var root = ExpressionTreeNode.CreateAnd(node, ParseNot());
        node = root;

        _tokenReader.NextToken();

        while (true)
        {
            if (_tokenReader.Token != Token.And) return root;

            _tokenReader.NextToken();

            var child1 = node.Child2;

            node.Child2 = ExpressionTreeNode.CreateAnd(child1, ParseNot());

            node = node.Child2;

            _tokenReader.NextToken();
        }
    }


    private ExpressionTreeNode ParseNot()
    {
        if (_tokenReader.Token != Token.Not) return ParseTerm();

        _tokenReader.NextToken();

        var term = ParseNot();

        return ExpressionTreeNode.CreateNot(term);
    }

    private ExpressionTreeNode ParseTerm()
    {
        if (_tokenReader.Token != Token.Term) throw new Exception("Invalid search string format");

        return ExpressionTreeNode.CreateTerm(_tokenReader.Term);
    }

    private ExpressionTreeNode OptimizeTree(ExpressionTreeNode? node)
    {
        if (node is null)
        {
            return null;
        }

        if (node.Operation is "ALL" or "ZERO")
        {
            return node;
        }

        var count = 0;
        node = OptimizeTreeInternal(node, ref count);

        while (count != 0)
        {
            count = 0;
            node = OptimizeTreeInternal(node, ref count);
        }

        return node;
    }

    private ExpressionTreeNode OptimizeTreeInternal(ExpressionTreeNode? node, ref int count)
    {
        if (node is null)
        {
            return null;
        }

        foreach (var action in _optimizeActions) node = action(node, ref count);

        node.Child1 = OptimizeTreeInternal(node.Child1, ref count);
        node.Child2 = OptimizeTreeInternal(node.Child2, ref count);

        return node;
    }

    /// <summary>
    ///     AND(NOT,NOT) -> NOT(OR)
    /// </summary>
    /// <param name="node"></param>
    private static ExpressionTreeNode TwoNotInAndRule(ExpressionTreeNode? node, ref int count)
    {
        if (node is null)
        {
            return null;
        }

        if (node.Operation != "AND" || node.Child1.Operation != "NOT" || node.Child2.Operation != "NOT")
        {
            return node;
        }
        
        var andChild = ExpressionTreeNode.CreateOr(node.Child1.Child1, node.Child2.Child1);

        node = ExpressionTreeNode.CreateNot(andChild);

        count++;

        return node;
    }

    /// <summary>
    ///     AND(NOT, term) -> AND(term, NOT).
    ///     It helps execute note more faster.
    /// </summary>
    /// <param name="node"></param>
    private static ExpressionTreeNode ReplaceFirstNotInAndRule(ExpressionTreeNode? node, ref int count)
    {
        if (node is null)
        {
            return null;
        }

        if (node.Operation != "AND" || node.Child1.Operation != "NOT" || node.Child2.Operation == "NOT")
        {
            return node;
        }
        
        (node.Child1, node.Child2) = (node.Child2, node.Child1);

        count++;

        return node;
    }

    /// <summary>
    ///     OR(NOT, NOT) -> ALL - if terms are not equal
    ///     OR(NOT, NOT) -> NOT(term) - if terms are equal
    /// </summary>
    /// <param name="node"></param>
    private static ExpressionTreeNode TwoNotInOrRule(ExpressionTreeNode? node, ref int count)
    {
        if (node is null)
        {
            return null;
        }

        if (node.Operation != "OR" || node.Child1.Operation != "NOT" || node.Child2.Operation != "NOT")
        {
            return node;
        }
       
        node = node.Child1.Child1.Term == node.Child2.Child1.Term 
            ? ExpressionTreeNode.CreateNot(node.Child1.Child1) 
            : ExpressionTreeNode.CreateAllNode();

        count++;

        return node;
    }

    /// <summary>
    ///     AND(Node, ALL) -> Node
    ///     OR(Node, ALL) -> ALL
    ///     NOT(ALL) -> ZERO
    /// </summary>
    /// <returns></returns>
    private static ExpressionTreeNode HandleAllNode(ExpressionTreeNode? node, ref int count)
    {
        if (node is null)
        {
            return null;
        }

        if (node.Child1?.Operation != "ALL" && node.Child2?.Operation != "ALL")
        {
            return node;
        }
       
        var otherNode = node.Child1.Operation == "ALL" ? node.Child2 : node.Child1;

        node = node.Operation switch
        {
            "AND" => otherNode,
            "OR" => ExpressionTreeNode.CreateAllNode(),
            "NOT" => ExpressionTreeNode.CreateZeroNode(),
            _ => node
        };

        count++;

        return node;
    }

    /// <summary>
    ///     AND(Node, ZERO) -> Zero
    ///     OR(Node, ZERO) -> Node
    ///     NOT(ZERO) -> ALL
    /// </summary>
    /// <returns></returns>
    private static ExpressionTreeNode HandleZeroNode(ExpressionTreeNode? node, ref int count)
    {
        if (node is null)
        {
            return null;
        }

        if (node.Child1?.Operation != "ZERO" && node.Child2?.Operation != "ZERO")
        {
            return node;
        }
        
        var otherNode = node.Child1.Operation == "ZERO" ? node.Child2 : node.Child1;

        node = node.Operation switch
        {
            "AND" => ExpressionTreeNode.CreateAllNode(),
            "OR" => otherNode,
            "NOT" => ExpressionTreeNode.CreateAllNode(),
            _ => node
        };

        count++;

        return node;
    }

    private delegate ExpressionTreeNode OptimizeAction(ExpressionTreeNode node, ref int count);
}