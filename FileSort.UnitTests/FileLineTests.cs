using FileSort.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [TestCase("1.X", 1, "X", TestName = "ShouldParseShortStringWothoutSpace")]
        [TestCase("1. X", 1, "X", TestName = "ShouldParseShortStringWithSpace")]
        [TestCase("2. Banana is yellow", 2, "Banana is yellow", TestName = "ShouldParseSimpleString")]
        [TestCase("2.  Banana is yellow", 2, "Banana is yellow", TestName = "ShouldParseSimpleStringWithDoubleSpace")]
        [TestCase("2. Banana is yellow ", 2, "Banana is yellow", TestName = "ShouldParseSimpleStringWithSpaceAtTheEnd")]
        [TestCase("2. Banana  yellow", 2, "Banana  yellow", TestName = "ShouldParseSimpleStringWithDoubleSpaceInTheMiddle")]
        [TestCase(" 2. Banana is yellow", 2, "Banana is yellow", TestName = "ShouldParseSimpleStringWithSpaceAtStart")]
        [TestCase("2147483647. Apple", int.MaxValue, "Apple", TestName = "ShouldNotParseStringWithMaxNumber")]
        public void ShouldParseCorrectString(string inputString, int number, string name)
        {
            Assert.IsTrue(FileLine.TryParse(inputString, out var fileName));
            Assert.AreEqual(number, fileName.Number);
            Assert.AreEqual(name, fileName.Name);

            if (inputString != null)
            {
                using (var streamReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(inputString))))
                {
                    Assert.IsTrue(FileLine.TryParse(streamReader, out fileName));
                    Assert.AreEqual(number, fileName.Number);
                    Assert.AreEqual(name, fileName.Name);
                }
            }
        }
    }
}
