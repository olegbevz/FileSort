using NUnit.Framework;
using System;

namespace FileGenerate.UnitTests
{
    [TestFixture]
    public abstract class RandomStringFactoryBaseTests
    {
        private IRandomStringFactory _stringFactory; 

        [SetUp]
        public void SetUp()
        {
            _stringFactory = CreateStringFactory();
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(1000001)]
        public void ShouldNotCreateTooSmallString(int length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _stringFactory.Create(length));
        }

        [TestCase(4, TestName = "ShouldCreateSmallestPossibleString")]
        [TestCase(10)]
        [TestCase(1000000, TestName = "ShouldCreateLargestPossibleString")]
        public void ShouldCreateStringWithLength(int length)
        {
            var @string = _stringFactory.Create(length);
            Assert.AreEqual(length, @string.Length, $"String '{@string}' should have length {length}");
        }

        protected abstract IRandomStringFactory CreateStringFactory();
    }

    public class SequenceStringFactoryTests : RandomStringFactoryBaseTests
    {
        protected override IRandomStringFactory CreateStringFactory()
        {
            return new SequenceStringFactory();
        }
    }

    public class RandomStringFactoryTests : RandomStringFactoryBaseTests
    {
        protected override IRandomStringFactory CreateStringFactory()
        {
            return new RandomStringFactory();
        }
    }

    public class BogusStringFactoryTests : RandomStringFactoryBaseTests
    {
        protected override IRandomStringFactory CreateStringFactory()
        {
            return new BogusStringFactory();
        }
    }

    public class AutoFixtureStringFactoryTests : RandomStringFactoryBaseTests
    {
        protected override IRandomStringFactory CreateStringFactory()
        {
            return new AutoFixtureStringFactory();
        }
    }

    public class ConstantStringFactoryTests : RandomStringFactoryBaseTests
    {
        protected override IRandomStringFactory CreateStringFactory()
        {
            return new ConstantStringFactory(32, "Cherry is the best");
        }
    }
}
