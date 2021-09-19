using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace StringComparisonCompiler
{
    internal enum MatchNodeCompilerInputType
    {
        String,
        CharSpan
    }

    internal class MatchNodeCompiler<TEnum>
    {
        private readonly LabelTarget _returnTarget;
        private readonly ParameterExpression _input;
        private readonly MatchNodeCompilerInputType _inputType;
        private readonly MethodInfo? _subMethodInfo;

        // Avoiding static in generic class.
        private readonly MethodInfo _getCharMethodInfo = typeof(SpanHelper)
            .GetMethod(
                nameof(SpanHelper.GetChar),
                BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new NotSupportedException($"Unable to locate {nameof(SpanHelper.GetChar)}");

        internal static readonly Expression DefaultResult = typeof(TEnum) == typeof(long)
            ? Expression.Constant(-1L)
            : Expression.Constant((TEnum?)default, typeof(TEnum?));

        public MatchNodeCompiler(
            LabelTarget returnTarget,
            ParameterExpression input,
            MatchNodeCompilerInputType inputType,
            MethodInfo? subMethodInfo)
        {
            _returnTarget = returnTarget;
            _input = input;
            _inputType = inputType;
            _subMethodInfo = subMethodInfo;
        }

        public Expression Compile(
            MatchNode<TEnum> target,
            int inputIndex,
            MinMax previousPathLengths)
        {
            var voidExpression = Expression.Block(Array.Empty<Expression>());
            var lengthExpression = Expression.Property(_input, nameof(ReadOnlySpan<char>.Length));

            Expression GetCharExpression()
            {
                var indexExp = Expression.Constant(inputIndex);

                Expression singleChrExpression = _inputType switch
                {
                    MatchNodeCompilerInputType.CharSpan => Expression.Call(_getCharMethodInfo, _input, indexExp),
                    MatchNodeCompilerInputType.String => Expression.Property(_input, "Chars", indexExp),
                    _ => throw new NotImplementedException(),
                };

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
}
