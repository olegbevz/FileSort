using System;
using System.Text;

namespace FileGenerate
{
    public abstract class RandomStringFactoryBase : IRandomStringFactory
    {
        private static string[] _words = new string[]
        {
            "Apple",
            "Something",
            "Cherry",
            "Banana",
            "is",
            "the",
            "best",
            "yellow"
        };

        private const string Separator = ". ";
        private const int SeparatorLength = 2;
        private const int MinStringLength = 4;
        private const int MaxNumber = 1000000;
        private const int MaxSentenceLength = 10;
        private const int MaxStringLength = 1000000;

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public string Create()
        {
            _stringBuilder.Append(GetNextNumber(MaxNumber));
            _stringBuilder.Append(". ");
            AppendNextName(_stringBuilder);

            var @string = _stringBuilder.ToString();
            _stringBuilder.Clear();
            return @string;
        }

        public string Create(int length)
        {
            if (length < MinStringLength || length > MaxStringLength)
                throw new ArgumentOutOfRangeException(nameof(length));

            var maxNumber = (int)Math.Min(Math.Pow(10, length - SeparatorLength - 1), MaxNumber);
            var number = GetNextNumber(maxNumber).ToString();
            _stringBuilder.Append(number);
            _stringBuilder.Append(Separator);
            AppendNextName(_stringBuilder, length - _stringBuilder.Length);

            var @string = _stringBuilder.ToString();
            _stringBuilder.Clear();
            return @string;
        }

        protected abstract int GetNextNumber(int maxNumber);
        protected virtual int GetNextSentenceLength(int maxSentenceLength)
        {
            throw new NotImplementedException();
        }
        protected virtual int GetNextWordNumber(int maxWordNumber)
        {
            throw new NotImplementedException();
        }

        protected virtual void AppendNextName(StringBuilder stringBuilder, int length)
        {
            int leftLength = length;

            while (leftLength > 0)
            {
                var word = GetNextWord();
                if (word.Length > leftLength)
                    word = word.Substring(0, leftLength);

                stringBuilder.Append(word);
                leftLength -= word.Length;
                if (leftLength > 0)
                {
                    stringBuilder.Append(' ');
                    leftLength--;
                }
            }
        }

        protected virtual void AppendNextName(StringBuilder stringBuilder)
        {
            int sentenceLength = GetNextSentenceLength(MaxSentenceLength);

            for (int i = 0; i < sentenceLength; i++)
            {                             
                stringBuilder.Append(GetNextWord());
                if (i < sentenceLength - 1)
                {
                    stringBuilder.Append(' ');
                }
            }
        }

        protected virtual string GetNextWord()
        {
            return _words[GetNextWordNumber(_words.Length - 1)];
        }
    }
}
