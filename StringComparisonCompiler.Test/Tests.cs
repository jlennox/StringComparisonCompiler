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
            var culture = caseInsensitive
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.CurrentCulture;

            var compiled = StringComparisonCompiler<Foobar>.Compile(culture, out var expression);
            var stringed = ExpressionStringify.Stringify(expression);

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
            var compiled = StringComparisonCompiler<Overlapped>.Compile(
                StringComparison.InvariantCultureIgnoreCase, out var expression);
            var stringed = ExpressionStringify.Stringify(expression);

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
                var compiled = StringComparisonCompiler<DuplicateNamesEnum>.Compile();
            });
        }

        [TestMethod]
        public void ArrayTest()
        {
            var arr = new[] { "foo", "bar", "baz" };
            var compiled = StringComparisonCompiler.Compile(
                arr, StringComparison.CurrentCulture, false, out var expression);
            var stringed = ExpressionStringify.Stringify(expression);
            Assert.AreEqual(0, compiled("foo"));
            Assert.AreEqual(1, compiled("bar"));
            Assert.AreEqual(2, compiled("baz"));
            Assert.AreEqual(-1, compiled("Not found"));
        }
    }
}
