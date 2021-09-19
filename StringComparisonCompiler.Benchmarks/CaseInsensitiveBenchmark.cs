using System;
using BenchmarkDotNet.Attributes;

namespace StringComparisonCompiler.Benchmarks
{
    public class CaseInsensitiveBenchmark
    {
        private const StringComparison _comparison = StringComparison.InvariantCultureIgnoreCase;

        private static readonly StringComparisonCompiler<TestingEnum>.SpanStringComparer _compiledSpan = StringComparisonCompiler<TestingEnum>.CompileSpan(_comparison);
        private static readonly StringComparisonCompiler<TestingEnum>.StringComparer _compiled = StringComparisonCompiler<TestingEnum>.Compile(_comparison);
        private static readonly MatchTree<KeywordsEnum> _trie = new(StringComparison.InvariantCultureIgnoreCase);

        [Params("While", "ForEach", "Foobar", "DoesNotExist")]
        public string N;

        [Benchmark]
        public TestingEnum CompiledSpan()
        {
            return _compiledSpan(N);
        }

        [Benchmark]
        public TestingEnum Compiled()
        {
            return _compiled(N);
        }

        [Benchmark]
        public bool Trie()
        {
            return _trie.ContainsWord(N);
        }

        [Benchmark]
        public TestingEnum Switch()
        {
            switch (N.ToUpperInvariant())
            {
                case "TEST": return TestingEnum.Test;
                case "FOR": return TestingEnum.For;
                case "FOREACH": return TestingEnum.ForEach;
                case "WHEN": return TestingEnum.When;
                case "WHILE": return TestingEnum.While;
                case "RETURN": return TestingEnum.Return;
                default: return TestingEnum.Test;
            };
        }

        [Benchmark]
        public TestingEnum If()
        {
            if (string.Equals("TEST", N, StringComparison.InvariantCultureIgnoreCase)) return TestingEnum.Test;
            if (string.Equals("FOR", N, StringComparison.InvariantCultureIgnoreCase)) return TestingEnum.For;
            if (string.Equals("FOREACH", N, StringComparison.InvariantCultureIgnoreCase)) return TestingEnum.ForEach;
            if (string.Equals("WHEN", N, StringComparison.InvariantCultureIgnoreCase)) return TestingEnum.When;
            if (string.Equals("WHILE", N, StringComparison.InvariantCultureIgnoreCase)) return TestingEnum.While;
            if (string.Equals("RETURN", N, StringComparison.InvariantCultureIgnoreCase)) return TestingEnum.Return;
            return TestingEnum.Test;
        }
    }
}