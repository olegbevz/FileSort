using FileSort.Core;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace FileSort.UnitTests
{
    [TestFixture]
    public class FileLineTests
    {
        [TestCase("", TestName = "ShouldNotParseEmptyString")]
        [TestCase(null, TestName = "ShouldNotParseNullString")]
        [TestCase(" ", TestName = "ShouldNotParseSpaceString")]
        [TestCase("1234", TestName = "ShouldNotParseNumber")]
        [TestCase("Apple", TestName = "ShouldNotParseStringWithoutNumber")]
        [TestCase("123 Apple", TestName = "ShouldNotParseStringWithoutDot")]
        [TestCase("2147483648. Apple", TestName = "ShouldNotParseStringWithLargeNumber")]
        [TestCase("21474836489. Apple", TestName = "ShouldNotParseStringWithExtraLargeNumber")]
        public void ShouldNotParseIncorrectString(string inputString)
        {
            Assert.IsFalse(FileLine.TryParse(inputString, out var fileLine));
            Assert.AreEqual(FileLine.None, fileLine);

            if (inputString != null)
            {
                using (var streamReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(inputString))))
                {
                    Assert.IsFalse(FileLine.TryParse(streamReader, out fileLine));
                    Assert.AreEqual(FileLine.None, fileLine);
                }
            }
        }

        [TestCase("1.X", 1, "X", 5, TestName = "ShouldParseShortStringWithoutSpace")]
        [TestCase("1. X", 1, "X", 5, TestName = "ShouldParseShortStringWithSpace")]
        [TestCase("2. Banana is yellow", 2, "Banana is yellow", 20, TestName = "ShouldParseSimpleString")]
        [TestCase("2.  Banana is yellow", 2, "Banana is yellow", 20, TestName = "ShouldParseSimpleStringWithDoubleSpace")]
        [TestCase("2. Banana is yellow ", 2, "Banana is yellow ", 21, TestName = "ShouldParseSimpleStringWithSpaceAtTheEnd")]
        [TestCase("2. Banana  yellow", 2, "Banana  yellow", 18, TestName = "ShouldParseSimpleStringWithDoubleSpaceInTheMiddle")]
        [TestCase(" 2. Banana is yellow", 2, "Banana is yellow", 20, TestName = "ShouldParseSimpleStringWithSpaceAtStart")]
        [TestCase("2147483647. Apple", int.MaxValue, "Apple", 9, TestName = "ShouldParseStringWithMaxNumber")]
        public void ShouldParseCorrectString(string inputString, int number, string name, long size)
        {
            Assert.IsTrue(FileLine.TryParse(inputString, out var fileLine));
            Assert.AreEqual(number, fileLine.Number);
            Assert.AreEqual(name, fileLine.Name);
            Assert.AreEqual(size, fileLine.Size);

            if (inputString != null)
            {
                using (var streamReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(inputString))))
                {
                    Assert.IsTrue(FileLine.TryParse(streamReader, out fileLine));
                    Assert.AreEqual(number, fileLine.Number);
                    Assert.AreEqual(name, fileLine.Name);
                    Assert.AreEqual(size, fileLine.Size);
                }
            }
        }
    }
}
