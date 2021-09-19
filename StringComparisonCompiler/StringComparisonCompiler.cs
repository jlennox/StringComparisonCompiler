using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StringComparisonCompiler.Test")]
[assembly: InternalsVisibleTo("StringComparisonCompiler.Benchmarks")]

namespace StringComparisonCompiler
{
    public static class StringComparisonCompiler<TEnum>
        where TEnum : struct, Enum
    {
        public delegate TEnum SpanStringComparer(ReadOnlySpan<char> input);
        public delegate TEnum StringCamparer(ReadOnlySpan<char> input);

        public static SpanStringComparer CompileSpan(
            StringComparison comparison = StringComparison.CurrentCulture,
            bool testStartsWith = false)
        {
            var tree = new MatchTree<TEnum>(comparison, testStartsWith);
            return tree.Compile();
        }

        public static StringCamparer Compile(
            StringComparison comparison = StringComparison.CurrentCulture,
            bool testStartsWith = false)
        {
            throw new NotImplementedException();
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