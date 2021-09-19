using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StringComparisonCompiler.Test
{
    public enum Foobar
    {
        Default,
        [System.ComponentModel.Description("testing")]
        Testing,
        [System.ComponentModel.Description("test0ng")]
        Test0ng,
        [System.ComponentModel.Description("test0ng-longer")]
        Test0ngLonger,
    }

    [TestClass]
    public class Tests
    {
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Test(bool caseInsensitive)
        {
            var compiler = new MatchTree<Foobar>(
                caseInsensitive
                    ? StringComparison.InvariantCultureIgnoreCase
                    : StringComparison.CurrentCulture,
                false);

            var compiled = compiler.Compile();
            var stringed = ExpressionStringify.Stringify(compiler.Exp);

            Compiled("testing");

            Assert.AreEqual(Foobar.Testing, compiled("testing"));
            Assert.AreEqual(caseInsensitive ? Foobar.Testing: Foobar.Default, compiled("tEsting"));
            Assert.AreEqual(caseInsensitive ? Foobar.Testing : Foobar.Default, compiled("TESTING"));
            Assert.AreEqual(Foobar.Test0ng, compiled("test0ng"));
            Assert.AreEqual(Foobar.Test0ngLonger, compiled("test0ng-longer"));
            Assert.AreEqual(Foobar.Default, compiled("t0sting"));
            Assert.AreEqual(Foobar.Default, compiled("testing2"));
            Assert.AreEqual(Foobar.Default, compiled("failing"));
            Assert.AreEqual(Foobar.Default, compiled("failing"));

            // Ensure nothing weird happens at check boundaries.
            for (var i = "testing".Length - 1; i >= 0; --i)
            {
                var substringA = "testing"[..i];
                Assert.AreEqual(Foobar.Default, compiled(substringA));
                var substringB = "test0ng"[..i];
                Assert.AreEqual(Foobar.Default, compiled(substringB));
            }
        }

        enum Overlapped
        {
            A,
            AA,
            AAA,
        }

        [TestMethod]
        public void OverlappingNames()
        {
            var compiler = new MatchTree<Overlapped>(
                StringComparison.InvariantCultureIgnoreCase,
                false);

            var compiled = compiler.Compile();
            var stringed = ExpressionStringify.Stringify(compiler.Exp);
            var description = compiler.GetDescription();

            // The compiler has to be able to differentiate when items are
            // partial.

            Assert.AreEqual(Overlapped.A, compiled("A"));
            Assert.AreEqual(Overlapped.AA, compiled("AA"));
            Assert.AreEqual(Overlapped.AAA, compiled("AAA"));
        }

        enum DuplicateNamesEnum
        {
            [System.ComponentModel.Description("duplicate")]
            Foo,
            [System.ComponentModel.Description("duplicate")]
            Bar
        }

        [TestMethod]
        public void DuplicateNames()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                var compiler = new MatchTree<DuplicateNamesEnum>(
                    StringComparison.InvariantCultureIgnoreCase,
                    false);

                var compiled = compiler.Compile();
            });
        }

        private static Foobar Compiled(string s)
        {
            if (s.Length < 7 || s.Length > 14)
            {
                return StringComparisonCompiler.Test.Foobar.Default;
            }

            switch (s[0])
            {
                case 'D':
                    {
                        if (s.Length != 7)
                        {
                            return StringComparisonCompiler.Test.Foobar.Default;
                        }
                        return (s[1] == 'E' && s[2] == 'F' && s[3] == 'A' && s[4] == 'U' && s[5] == 'L' && s[6] == 'T')
                    ? (StringComparisonCompiler.Test.Foobar.Default)
                    : (StringComparisonCompiler.Test.Foobar.Default);
                        break;
                    }
                case 'T':
                    {
                        if (s.Length < 3)
                        {
                            return StringComparisonCompiler.Test.Foobar.Default;
                        }
                        if (!(s[1] == 'E' && s[2] == 'S'))
                        {
                            return StringComparisonCompiler.Test.Foobar.Default;
                        }


                        switch (s[3])
                        {
                            case 'T':
                                {
                                    switch (s[4])
                                    {
                                        case 'I':
                                            {
                                                if (s.Length != 7)
                                                {
                                                    return StringComparisonCompiler.Test.Foobar.Default;
                                                }
                                                return (s[5] == 'N' && s[6] == 'G')
                            ? (StringComparisonCompiler.Test.Foobar.Testing)
                            : (StringComparisonCompiler.Test.Foobar.Default);
                                                break;
                                            }
                                        case '0':
                                            {
                                                if (s.Length < 7)
                                                {
                                                    return StringComparisonCompiler.Test.Foobar.Default;
                                                }
                                                if (!(s[5] == 'N' && s[6] == 'G'))
                                                {
                                                    return StringComparisonCompiler.Test.Foobar.Default;
                                                }

                                                if (s.Length == 7)
                                                {
                                                    return StringComparisonCompiler.Test.Foobar.Test0ng;
                                                }
                                                switch (s[7])
                                                {
                                                    case '-':
                                                        {
                                                            if (s.Length != 14)
                                                            {
                                                                return StringComparisonCompiler.Test.Foobar.Default;
                                                            }
                                                            return (s[8] == 'L' && s[9] == 'O' && s[10] == 'N' && s[11] == 'G' && s[12] == 'E' && s[13] == 'R')
                                ? (StringComparisonCompiler.Test.Foobar.Test0ngLonger)
                                : (StringComparisonCompiler.Test.Foobar.Default);
                                                            break;
                                                        }
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }
            return default;
        }
    }
}
