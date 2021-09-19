using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StringComparisonCompiler
{
    internal record MinMax(int Min, int Max);

    internal class MatchNode<TEnum>
    {
        public char Char { get; }
        public Dictionary<char, MatchNode<TEnum>> Children { get; }
        public bool IsTerminal { get; private set; }
        public TEnum? Value { get; }

        public MatchNode<TEnum>? FirstChild => Children.Values.FirstOrDefault();

        public MatchNode(char c, bool isTerminal = false, TEnum? value = default)
        {
            Char = c;
            Value = value;
            Children = new Dictionary<char, MatchNode<TEnum>>();
            IsTerminal = isTerminal;
        }

        public MatchNode<TEnum> GetChildOrCreate(char c, bool isTerminal, TEnum? value)
        {
            if (Children.TryGetValue(c, out var node)) return node;

            node = new MatchNode<TEnum>(c, isTerminal, value);
            node.IsTerminal = node.IsTerminal || isTerminal;
            Children[c] = node;
            return node;
        }

        // Does this node, and at least one descendant, have only a single child.
        // If so, this path can be condensed.
        public bool CanCondense()
        {
            var count = 0;
            for (var cur = this;
                cur != null && cur.Children.Count == 1 && !cur.IsTerminal;
                cur = cur.FirstChild)
            {
                if (++count > 1) return true;
            }
            return false;
        }

        // Returns the min and max depth from this node.
        public MinMax GetPathLengths(MinMax counts, int? terminalMin = null)
        {
            if (Children.Count == 0) return counts;
            // If a terminal node was found on this path, then stop increasing the min.
            // This happens when one value is a subset of another. Ie, "Foo" and "Foobar" both exist in the tree
            // as terminals.
            if (IsTerminal) terminalMin = counts.Min;

            var localcounts = new MinMax(counts.Min + 1, counts.Max + 1);
            var hasResult = false;
            foreach (var kid in Children)
            {
                var (kidMin, kidMax) = kid.Value.GetPathLengths(localcounts, terminalMin);
                var min = terminalMin ?? (hasResult ? Math.Min(kidMin, counts.Min) : kidMin);
                var max = hasResult ? Math.Max(kidMax, counts.Max) : kidMax;
                counts = new MinMax(min, max);
                hasResult = true;
            }

            return counts;
        }

        public Expression Compile(
            LabelTarget returnTarget,
            ParameterExpression input,
            MatchNodeCompilerInputType inputType,
            MethodInfo? subMethodInfo)
        {
            var compiler = new MatchNodeCompiler<TEnum>(returnTarget, input, inputType, subMethodInfo);
            return compiler.Compile(this, 0, new MinMax(0, 0));
        }

        // Exists for profiling purposes, to compare the compiled vs the soft execution speeds.
        internal bool ContainsTerminal(string word, MethodInfo? transform)
        {
            var node = this;

            for (var i = 0; i < word.Length; ++i)
            {
                var chr = transform != null ? char.ToUpperInvariant(word[i]) : word[i];
                if (!node.Children.TryGetValue(chr, out node)) return false;
                var isTerminal = i == word.Length - 1;
                if (isTerminal && node.IsTerminal) return true;
            }

            return false;
        }

        // For debug purposes.
        internal string GetDescription(int indent = 0)
        {
            var terminalPrefix = IsTerminal ? "T" : "";
            var node = new string(' ', indent * 2) + $"{terminalPrefix}'{(Char == '\0' ? ' ' : Char)}'{Value}:\n";
            return Children.Count == 0
                ? node
                : node + string.Concat(Children.Values.Select(t => t.GetDescription(indent + 1)));
        }
    }
}