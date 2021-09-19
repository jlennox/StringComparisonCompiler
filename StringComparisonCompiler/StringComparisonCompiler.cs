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
            StringComparison comparison = StringComparison.CurrentCulture)
        {
            return CompileSpan(comparison, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SpanStringComparer CompileSpan(
            StringComparison comparison,
            out Expression expression)
        {
            var tree = new MatchTree<TEnum>(comparison);
            return tree.Compile<SpanStringComparer>(MatchNodeCompilerInputType.CharSpan, out expression);
        }

        public static StringComparer Compile(
            StringComparison comparison = StringComparison.CurrentCulture)
        {
            return Compile(comparison, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static StringComparer Compile(
            StringComparison comparison,
            out Expression expression)
        {
            var tree = new MatchTree<TEnum>(comparison);
            return tree.Compile<StringComparer>(MatchNodeCompilerInputType.String, out expression);
        }
    }

    public static class StringComparisonCompiler
    {
        public delegate long SpanStringComparer(ReadOnlySpan<char> input);
        public delegate long StringComparer(string input);

        public static StringComparer Compile(
            string[] input,
            StringComparison comparison = StringComparison.CurrentCulture)
        {
            return Compile(input, comparison, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static StringComparer Compile(
            string[] input,
            StringComparison comparison,
            out Expression expression)
        {
            var tree = new MatchTree(input, comparison);
            return tree.Compile<StringComparer>(MatchNodeCompilerInputType.String, out expression);
        }

        public static SpanStringComparer CompileSpan(
            string[] input,
            StringComparison comparison = StringComparison.CurrentCulture)
        {
            return CompileSpan(input, comparison, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static SpanStringComparer CompileSpan(
            string[] input,
            StringComparison comparison,
            out Expression expression)
        {
            var tree = new MatchTree(input, comparison);
            return tree.Compile<SpanStringComparer>(MatchNodeCompilerInputType.CharSpan, out expression);
        }
    }
}