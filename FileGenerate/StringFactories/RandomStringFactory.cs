﻿using System;

namespace FileGenerate
{
    public class RandomStringFactory : RandomStringFactoryBase
    {
        private static Random _random = new Random();
        private readonly int _maxNumber;
        private readonly int _maxSentenceLength;

        public RandomStringFactory(int maxNumber = 1000000, int maxSentenceLength = 10)
        {
            _maxNumber = maxNumber;
            _maxSentenceLength = maxSentenceLength;
        }

        protected override int GetNextNumber()
        {
            return _random.Next(_maxNumber);
        }

        protected override int GetNextSentenceLength()
        {
            return _random.Next(_maxSentenceLength);
        }

        protected override int GetNextWordNumber(int maxWordNumber)
        {
            return _random.Next(maxWordNumber);
        }
    }
}