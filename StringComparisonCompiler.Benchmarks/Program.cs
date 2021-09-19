using System;
using System.Linq.Expressions;
using BenchmarkDotNet.Running;

namespace StringComparisonCompiler.Benchmarks
{
    class Program
    {
        static unsafe void Test()
        {
            ReadOnlySpan<char> foovar = "  ";

            var param = Expression.Parameter(typeof(IntPtr), "s");
            var arrayindex = Expression.ArrayIndex(param, Expression.Constant(2));

            var parameter = Expression.Parameter(typeof(ReadOnlySpan<char>), "s");
            var expression = Expression.Block(arrayindex);
            var compiled = Expression.Lambda<Action<IntPtr>>(expression, parameter).Compile(false);

            fixed (char* ptr2 = " ello World")
            {
                compiled((IntPtr)ptr2);
            }

            // call         instance !0/*char*/& modreq ([System.Runtime]System.Runtime.InteropServices.InAttribute) valuetype [System.Runtime]System.ReadOnlySpan`1<char>::GetPinnableReference()
            //fixed (char* ptr = foovar) {
            //}
        }

        static unsafe void Main(string[] args)
        {
            //Test();
            //var compiler = new MatchTree<KeywordsEnum>(
            //    StringComparison.CurrentCulture,
            //    false);
            //
            //var compiled = compiler.Compile();
            //var stringed = ExpressionStringify.Stringify(compiler.Exp);

            var summaries = new[] {
                BenchmarkRunner.Run<Benchmarks>(),
                BenchmarkRunner.Run<LargeDictionaryBenchmarks>(),
                BenchmarkRunner.Run<CaseInsensitiveBenchmark>()
            };

            foreach (var sum in summaries)
            {
                Console.WriteLine(sum);
            }
        }
    }
}
