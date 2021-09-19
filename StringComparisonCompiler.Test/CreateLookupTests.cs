using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StringComparisonCompiler.Test
{
    enum CreateLookupEnum
    {
        [System.ComponentModel.Description("Long Description")]
        LongDescription,
        NoDescription
    }

    [TestClass]
    public class CreateLookupTests
    {
        [TestMethod]
        public void ValidateValues()
        {
            var lookup = MatchTree<CreateLookupEnum>.CreateEnumLookup();

            Assert.AreEqual(lookup.Count, 2);

            // Enum fields containing a Description use the description.
            Assert.AreEqual(lookup["Long Description"], CreateLookupEnum.LongDescription);

            // Ones lacking Description use the name of the enum.
            Assert.AreEqual(lookup["NoDescription"], CreateLookupEnum.NoDescription);
        }
    }
}