namespace FileGenerate
{
    public class SequenceStringFactory : RandomStringFactoryBase
    {
        private readonly int _numberStep;
        private readonly int _sentenceLengthStep;
        private readonly int _wordNumberStep;

        private int _numberCounter;
        private int _sentenceLegthCounter;
        private int _wordNumberCounter;

        public SequenceStringFactory(
            int numberStep = 23,
            int sentenceLengthStep = 3, 
            int wordNumberStep = 4)
        {
            _numberStep = numberStep;
            _sentenceLengthStep = sentenceLengthStep;
            _wordNumberStep = wordNumberStep;
        }

        protected override int GetNextNumber(int maxNumber)
        {
            return GetNextRandomNumber(ref _numberCounter, _numberStep, 0, maxNumber);
        }

        protected override int GetNextSentenceLength(int maxSentenceLegth)
        {
            return GetNextRandomNumber(ref _sentenceLegthCounter, _sentenceLengthStep, 1, maxSentenceLegth);
        }

        protected override int GetNextWordNumber(int maxWordNumber)
        {
            return GetNextRandomNumber(ref _wordNumberCounter, _wordNumberStep, 0, maxWordNumber);
        }

        private int GetNextRandomNumber(ref int counter, int step, int minValue, int maxValue)
        {
            counter += step;
            if (counter > maxValue)
                counter -= maxValue;
            else if (counter < minValue)
                counter += minValue;            

            return counter;
        }
    }
}
