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

        /// <summary>
        /// Compile string comparison comparer that accepts string input.
        /// </summary>
        /// <param name="comparison"></param>
        /// <returns>The compiled method. The method returns the matched enum or default(TEnum)</returns>
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

        /// <summary>
        /// Compile string comparison comparer that accepts ReadOnlySpan&lt;char&gt; input.
        /// </summary>
        /// <param name="comparison"></param>
        /// <returns>The compiled method. The method returns the matched enum or default(TEnum)</returns>
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
    }

    /// <summary>
    /// Compiles string comparers from an array as input.
    /// </summary>
    public static class StringComparisonCompiler
    {
        public delegate long SpanStringComparer(ReadOnlySpan<char> input);
        public delegate long StringComparer(string input);

        /// <summary>
        /// Compile a string comparison comparer that accepts string input.
        /// </summary>
        /// <param name="input">The array of input to compile to a comparison</param>
        /// <param name="comparison"></param>
        /// <returns>The compiled method. The method returns the matched index or -1 if not found.</returns>
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

        /// <summary>
        /// Compile a string comparison comparer that accepts ReadOnlySpan&lt;char&gt; input.
        /// </summary>
        /// <param name="input">The array of input to compile to a comparison</param>
        /// <param name="comparison"></param>
        /// <returns>The compiled method. The method returns the matched index or -1 if not found.</returns>
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