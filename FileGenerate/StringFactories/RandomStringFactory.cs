using System;

namespace FileGenerate
{
    public class RandomStringFactory : RandomStringFactoryBase
    {
        private readonly Random _random;

        public RandomStringFactory(Random random)
        {
            _random = random;
        }

        public RandomStringFactory()
        {
            _random = new Random();
        }

        protected override int GetNextNumber(int maxNumber)
        {
            return _random.Next(maxNumber);
        }

        protected override int GetNextSentenceLength(int maxSentenceLength)
        {
            return _random.Next(1, maxSentenceLength);
        }

        protected override int GetNextWordNumber(int maxWordNumber)
        {
            return _random.Next(maxWordNumber);
        }
    }
}
