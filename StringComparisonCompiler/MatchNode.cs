using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StringComparisonCompiler
{
    internal record MinMax(int Min, int Max);

    internal class MatchNode<TEnum>
        where TEnum : Enum
    {
        public char Char { get; }
        public Dictionary<char, MatchNode<TEnum>> Children { get; }
        public bool IsTerminal { get; private set; }
        public TEnum? Value { get; }

        public MatchNode(char c, bool isTerminal = false, TEnum? value = default)
        {
            Char = c;
            Value = value;
            Children = new Dictionary<char, MatchNode<TEnum>>();
            IsTerminal = isTerminal;
        }

        internal MatchNode<TEnum> GetChildOrCreate(char c, bool isTerminal, TEnum? value)
        {
            if (Children.TryGetValue(c, out var node)) return node;

            node = new MatchNode<TEnum>(c, isTerminal, value);
            node.IsTerminal = node.IsTerminal || isTerminal;
            Children[c] = node;
            return node;
        }

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

        public MatchNode<TEnum>? FirstChild => Children.Values.FirstOrDefault();

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

        internal static readonly Expression DefaultResult = Expression.Constant((TEnum?)default, typeof(TEnum?));

        internal string GetDescription(int indent = 0)
        {
            var terminalPrefix = IsTerminal ? "T" : "";
            var node = new string(' ', indent * 2) + $"{terminalPrefix}'{(Char == '\0' ? ' ' : Char)}'{Value}:\n";
            return Children.Count == 0
                ? node
                : node + string.Concat(Children.Values.Select(t => t.GetDescription(indent + 1)));
        }

        public Expression GetSwitch(
            LabelTarget returnTarget,
            int index,
            MinMax previousPathLengths,
            ParameterExpression input,
            bool isStartsWith,
            MethodInfo? subMethodInfo)
        {
            // GetSwitch is broken into a static because the node which is being referenced can change during
            // processing, which would make usage of `this` an overly easy bug to introduce.
            return GetSwitchCore(this, returnTarget, index, previousPathLengths, input, isStartsWith, subMethodInfo);
        }

        internal enum MatchNodeCompilerInputType
        {
            String,
            CharSpan
        }

        internal class MatchNodeCompiler
        {
            private readonly LabelTarget _returnTarget;
            private readonly ParameterExpression _input;
            private readonly MatchNodeCompilerInputType _inputType;
            private readonly bool _isStartsWith;
            private readonly MethodInfo? _subMethodInfo;

            public MatchNodeCompiler(
                LabelTarget returnTarget,
                ParameterExpression input,
                MatchNodeCompilerInputType inputType,
                bool isStartsWith,
                MethodInfo? subMethodInfo)
            {
                _returnTarget = returnTarget;
                _input = input;
                _inputType = inputType;
                _isStartsWith = isStartsWith;
                _subMethodInfo = subMethodInfo;
            }

            private Expression Compile(
                MatchNode<TEnum> target,
                int inputIndex,
                MinMax previousPathLengths)
            {
                var voidExpression = Expression.Block(Array.Empty<Expression>());
                var lengthExpression = Expression.Property(_input, nameof(ReadOnlySpan<char>.Length));

                Expression GetCharExpression()
                {
                    //var singleChrExpression = Expression.Property(input, "Item", Expression.Constant(index));
                    var method = typeof(SpanHelper).GetMethod("GetChar", BindingFlags.Static | BindingFlags.NonPublic);
                    var singleChrExpression = Expression.Call(method, _input, Expression.Constant(inputIndex));
                    return _subMethodInfo != null
                        ? Expression.Call(_subMethodInfo, singleChrExpression)
                        : singleChrExpression;
                }

                // Outside of the condensed code, any path which does a "return" must also include condensedPathExpression!
                // Example: return Block.Expression(condensedPathExpression, ...);
                var condensedPathExpression = voidExpression;

                // When there's a path of single children, condense the checks into a single statement instead of
                // continuing with the switch checks.
                if (target.CanCondense())
                {
                    Expression? lastExpression = null;
                    var exitImmediately = false;
                    for (var condensed = target.FirstChild;
                        condensed != null && condensed.Children.Count is 1 or 0;
                        condensed = condensed.FirstChild)
                    {
                        var singleSubChr = GetCharExpression();
                        var curExpression = Expression.Equal(singleSubChr, Expression.Constant(condensed.Char));

                        lastExpression = lastExpression == null
                            ? curExpression
                            : Expression.AndAlso(lastExpression, curExpression);

                        target = condensed;
                        ++inputIndex;

                        if (condensed.Children.Count == 0)
                        {
                            exitImmediately = true;
                            break;
                        }

                        // We have to break up terminal paths when they're not also leafs.
                        if (condensed.IsTerminal) break;
                    }

                    if (lastExpression == null)
                    {
                        throw new Exception("lastExpression == null. This should never happen.");
                    }

                    // Completed paths don't want to go back to the switch/terminal/etc selector. They also have a
                    // different/simplified length check.
                    if (exitImmediately)
                    {
                        // return (s.Length != 4) return default;
                        var exitLengthCheck = Expression.IfThen(
                            Expression.NotEqual(lengthExpression, Expression.Constant(inputIndex)),
                            Expression.Return(_returnTarget, DefaultResult));

                        // return (s[2] == 'A' &&  && s[3] == 'B' ...) ? Value : default;
                        return Expression.Block(
                            exitLengthCheck,
                            Expression.Return(
                                _returnTarget,
                                Expression.Condition(
                                    lastExpression,
                                    Expression.Constant(target.Value),
                                    DefaultResult)));
                    }

                    // if (s.Length < 4) return default;
                    var singlePathLengthCheck = Expression.IfThen(
                        Expression.LessThan(lengthExpression, Expression.Constant(inputIndex)),
                        Expression.Return(_returnTarget, DefaultResult));

                    condensedPathExpression = Expression.Block(
                        singlePathLengthCheck,
                        // if (!(s[2] == 'A' &&  && s[3] == 'B' ...)) return default;
                        Expression.IfThen(Expression.Not(lastExpression), Expression.Return(_returnTarget, DefaultResult))
                    );
                }

                var inputChar = GetCharExpression();

                // Only check string lengths if:
                // * The path's length requirements have changed. If they have not, then they were checked previously.
                // * Reduce the checking as much as possible.
                var pathLengths = target.GetPathLengths(new MinMax(inputIndex, inputIndex));
                Expression? lengthCheckExact = pathLengths.Min == pathLengths.Max && pathLengths != previousPathLengths
                    ? Expression.IfThen(
                        Expression.NotEqual(lengthExpression,
                            Expression.Constant(pathLengths.Min)),
                        Expression.Return(_returnTarget, DefaultResult))
                    : null;

                var newMin = pathLengths.Min != previousPathLengths.Min;
                var newMax = pathLengths.Max != previousPathLengths.Max;
                var minCheck = Expression.LessThan(lengthExpression, Expression.Constant(pathLengths.Min));
                var maxCheck = Expression.GreaterThan(lengthExpression, Expression.Constant(pathLengths.Max));

                Expression? lengthCheckRange = (newMin, newMax) switch
                {
                    (true, true) => Expression.OrElse(minCheck, maxCheck),
                    (false, true) => minCheck,
                    (true, false) => maxCheck,
                    (false, false) => null
                };

                var lengthCheck = (lengthCheckExact, lengthCheckRange) switch
                {
                    (not null, _) => lengthCheckExact,
                    (null, not null) => Expression.IfThen(
                        lengthCheckRange,
                        Expression.Return(_returnTarget, DefaultResult)),
                    (null, null) => voidExpression
                };

                Expression terminalCheck = voidExpression;

                if (target.IsTerminal)
                {
                    // Leaf exit.
                    if (target.Children.Count == 0)
                    {
                        // return Value;
                        return Expression.Block(
                            condensedPathExpression,
                            lengthCheck,
                            Expression.Return(_returnTarget, Expression.Constant(target.Value)));
                    }

                    // if (s.Length == index) return Value;
                    terminalCheck = Expression.IfThen(
                        Expression.Equal(lengthExpression, Expression.Constant(inputIndex)),
                        Expression.Return(_returnTarget, Expression.Constant(target.Value)));
                }

                return Expression.Block(
                    condensedPathExpression,
                    lengthCheck,
                    terminalCheck,
                    Expression.Switch(inputChar, target.Children
                        .Select(t =>
                            Expression.SwitchCase(
                                Compile(t.Value, inputIndex + 1, pathLengths),
                                Expression.Constant(t.Key)))
                        .ToArray())
                );
            }
        }

        private static Expression GetSwitchCore(
            MatchNode<TEnum> target,
            LabelTarget returnTarget,
            int inputIndex,
            MinMax previousPathLengths,
            ParameterExpression input,
            bool isStartsWith,
            MethodInfo? subMethodInfo)
        {
            var voidExpression = Expression.Block(Array.Empty<Expression>());
            var lengthExpression = Expression.Property(input, nameof(ReadOnlySpan<char>.Length));

            Expression GetCharExpression()
            {
                //var singleChrExpression = Expression.Property(input, "Item", Expression.Constant(index));
                var method = typeof(SpanHelper).GetMethod("GetChar", BindingFlags.Static | BindingFlags.NonPublic);
                var singleChrExpression = Expression.Call(method, input, Expression.Constant(inputIndex));
                return subMethodInfo != null
                    ? Expression.Call(subMethodInfo, singleChrExpression)
                    : singleChrExpression;
            }

            // Outside of the condensed code, any path which does a "return" must also include condensedPathExpression!
            // Example: return Block.Expression(condensedPathExpression, ...);
            var condensedPathExpression = voidExpression;

            // When there's a path of single children, condense the checks into a single statement instead of
            // continuing with the switch checks.
            if (target.CanCondense())
            {
                Expression? lastExpression = null;
                var exitImmediately = false;
                for (var condensed = target.FirstChild;
                    condensed != null && condensed.Children.Count is 1 or 0;
                    condensed = condensed.FirstChild)
                {
                    var singleSubChr = GetCharExpression();
                    var curExpression = Expression.Equal(singleSubChr, Expression.Constant(condensed.Char));

                    lastExpression = lastExpression == null
                        ? curExpression
                        : Expression.AndAlso(lastExpression, curExpression);

                    target = condensed;
                    ++inputIndex;

                    if (condensed.Children.Count == 0)
                    {
                        exitImmediately = true;
                        break;
                    }

                    // We have to break up terminal paths when they're not also leafs.
                    if (condensed.IsTerminal) break;
                }

                if (lastExpression == null)
                {
                    throw new Exception("lastExpression == null. This should never happen.");
                }

                // Completed paths don't want to go back to the switch/terminal/etc selector. They also have a
                // different/simplified length check.
                if (exitImmediately)
                {
                    // return (s.Length != 4) return default;
                    var exitLengthCheck = Expression.IfThen(
                        Expression.NotEqual(lengthExpression, Expression.Constant(inputIndex)),
                        Expression.Return(returnTarget, DefaultResult));

                    // return (s[2] == 'A' &&  && s[3] == 'B' ...) ? Value : default;
                    return Expression.Block(
                        exitLengthCheck,
                        Expression.Return(
                            returnTarget,
                            Expression.Condition(
                                lastExpression,
                                Expression.Constant(target.Value),
                                DefaultResult)));
                }

                // if (s.Length < 4) return default;
                var singlePathLengthCheck = Expression.IfThen(
                    Expression.LessThan(lengthExpression, Expression.Constant(inputIndex)),
                    Expression.Return(returnTarget, DefaultResult));

                condensedPathExpression = Expression.Block(
                    singlePathLengthCheck,
                    // if (!(s[2] == 'A' &&  && s[3] == 'B' ...)) return default;
                    Expression.IfThen(Expression.Not(lastExpression), Expression.Return(returnTarget, DefaultResult))
                );
            }

            var inputChar = GetCharExpression();

            // Only check string lengths if:
            // * The path's length requirements have changed. If they have not, then they were checked previously.
            // * Reduce the checking as much as possible.
            var pathLengths = target.GetPathLengths(new MinMax(inputIndex, inputIndex));
            Expression? lengthCheckExact = pathLengths.Min == pathLengths.Max && pathLengths != previousPathLengths
                ? Expression.IfThen(
                    Expression.NotEqual(lengthExpression,
                        Expression.Constant(pathLengths.Min)),
                    Expression.Return(returnTarget, DefaultResult))
                : null;

            var newMin = pathLengths.Min != previousPathLengths.Min;
            var newMax = pathLengths.Max != previousPathLengths.Max;
            var minCheck = Expression.LessThan(lengthExpression, Expression.Constant(pathLengths.Min));
            var maxCheck = Expression.GreaterThan(lengthExpression, Expression.Constant(pathLengths.Max));

            Expression? lengthCheckRange = (newMin, newMax) switch {
                (true, true) => Expression.OrElse(minCheck, maxCheck),
                (false, true) => minCheck,
                (true, false) => maxCheck,
                (false, false) => null
            };

            var lengthCheck = (lengthCheckExact, lengthCheckRange) switch
            {
                (not null, _) => lengthCheckExact,
                (null, not null) => Expression.IfThen(
                    lengthCheckRange,
                    Expression.Return(returnTarget, DefaultResult)),
                (null, null) => voidExpression
            };

            Expression terminalCheck = voidExpression;

            if (target.IsTerminal)
            {
                // Leaf exit.
                if (target.Children.Count == 0)
                {
                    // return Value;
                    return Expression.Block(
                        condensedPathExpression,
                        lengthCheck,
                        Expression.Return(returnTarget, Expression.Constant(target.Value)));
                }

                // if (s.Length == index) return Value;
                terminalCheck = Expression.IfThen(
                    Expression.Equal(lengthExpression, Expression.Constant(inputIndex)),
                    Expression.Return(returnTarget, Expression.Constant(target.Value)));
            }

            return Expression.Block(
                condensedPathExpression,
                lengthCheck,
                terminalCheck,
                Expression.Switch(inputChar, target.Children
                    .Select(t =>
                        Expression.SwitchCase(
                            t.Value.GetSwitch(returnTarget, inputIndex + 1, pathLengths, input, isStartsWith, subMethodInfo),
                            Expression.Constant(t.Key)))
                    .ToArray())
            );
        }
    }
}