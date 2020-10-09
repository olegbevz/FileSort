namespace FileGenerate
{
    public class SequenceStringFactory : RandomStringFactoryBase
    {
        private readonly int _maxNumber;
        private readonly int _numberStep;
        private readonly int _maxSentenceLegth;
        private readonly int _sentenceLengthStep;
        private readonly int _wordNumberStep;

        private int _numberCounter;
        private int _sentenceLegthCounter;
        private int _wordNumberCounter;

        public SequenceStringFactory(
            int maxNumber = 1000000, 
            int numberStep = 23,
            int maxSentenceLegth = 10,
            int sentenceLengthStep = 3, 
            int wordNumberStep = 4)
        {
            _maxNumber = maxNumber;
            _numberStep = numberStep;
            _maxSentenceLegth = maxSentenceLegth;
            _sentenceLengthStep = sentenceLengthStep;
            _wordNumberStep = wordNumberStep;
        }

        protected override int GetNextNumber()
        {
            return GetNextRandomNumber(ref _numberCounter, _numberStep, _maxNumber);
        }

        protected override int GetNextSentenceLength()
        {
            return GetNextRandomNumber(ref _sentenceLegthCounter, _sentenceLengthStep, _maxSentenceLegth);
        }

        protected override int GetNextWordNumber(int maxWordNumber)
        {
            return GetNextRandomNumber(ref _wordNumberCounter, _wordNumberStep, maxWordNumber);
        }

        private int GetNextRandomNumber(ref int counter, int step, int maxValue)
        {
            counter += step;
            if (counter > maxValue)
                counter -= maxValue;

            return counter;
        }
    }
}
