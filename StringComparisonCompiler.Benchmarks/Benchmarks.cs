﻿using System;
using BenchmarkDotNet.Attributes;

namespace StringComparisonCompiler.Benchmarks
{
    public class Benchmarks
    {
        private static readonly StringComparisonCompiler<TestingEnum>.SpanStringComparison _compiled = StringComparisonCompiler<TestingEnum>.CompileSpan();
        private static readonly MatchTree<TestingEnum> _trie = new(StringComparison.CurrentCulture, false);

        [Params("While", "ForEach", "Foobar", "DoesNotExist")]
        public string N;

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
            switch (N)
            {
                case "Test": return TestingEnum.Test;
                case "For": return TestingEnum.For;
                case "ForEach": return TestingEnum.ForEach;
                case "When": return TestingEnum.When;
                case "While": return TestingEnum.While;
                case "Return": return TestingEnum.Return;
                default: return TestingEnum.Test;
            };
        }

        [Benchmark]
        public TestingEnum If()
        {
            if (N == "Test") return TestingEnum.Test;
            if (N == "For") return TestingEnum.For;
            if (N == "ForEach") return TestingEnum.ForEach;
            if (N == "When") return TestingEnum.When;
            if (N == "While") return TestingEnum.While;
            if (N == "Return") return TestingEnum.Return;
            return TestingEnum.Test;
        }
    }
}