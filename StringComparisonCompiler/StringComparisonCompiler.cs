using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StringComparisonCompiler.Test")]
[assembly: InternalsVisibleTo("StringComparisonCompiler.Benchmarks")]

namespace StringComparisonCompiler
{
    public static class StringComparisonCompiler<TEnum>
        where TEnum : struct, Enum
    {
        public delegate TEnum SpanStringComparer(ReadOnlySpan<char> input);
        public delegate TEnum StringComparer(string input);

        public static SpanStringComparer CompileSpan(
            StringComparison comparison = StringComparison.CurrentCulture,
            bool testStartsWith = false)
        {
            return CompileSpan(comparison, testStartsWith, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SpanStringComparer CompileSpan(
            StringComparison comparison,
            bool testStartsWith,
            out Expression expression)
        {
            var tree = new MatchTree<TEnum>(comparison, testStartsWith);
            return tree.Compile<SpanStringComparer>(MatchNodeCompilerInputType.CharSpan, out expression);
        }

        public static StringComparer Compile(
            StringComparison comparison = StringComparison.CurrentCulture,
            bool testStartsWith = false)
        {
            return Compile(comparison, testStartsWith, out _);
        }

        internal static StringComparer Compile(
            StringComparison comparison,
            bool testStartsWith,
            out Expression expression)
        {
            var tree = new MatchTree<TEnum>(comparison, testStartsWith);
            return tree.Compile<StringComparer>(MatchNodeCompilerInputType.String, out expression);
        }
    }

    public static class StringComparisonCompiler
    {
        public delegate int? SpanStringComparer(ReadOnlySpan<char> input);
        public delegate int? StringCamparer(ReadOnlySpan<char> input);

        public static SpanStringComparer CompileSpan(
            StringComparison comparison = StringComparison.CurrentCulture,
            bool testStartsWith = false)
        {
            throw new NotImplementedException();
        }

        public static StringCamparer Compile(
            StringComparison comparison = StringComparison.CurrentCulture,
            bool testStartsWith = false)
        {
            throw new NotImplementedException();
        }
    }
}