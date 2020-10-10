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

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public string Create()
        {
            _stringBuilder.Append(GetNextNumber());
            _stringBuilder.Append(". ");
            AppendNextName(_stringBuilder);

            var @string = _stringBuilder.ToString();
            _stringBuilder.Clear();
            return @string;
        }

        protected abstract int GetNextNumber();

        protected virtual void AppendNextName(StringBuilder stringBuilder)
        {
            int sentenceLength = GetNextSentenceLength();

            for (int i = 0; i < sentenceLength; i++)
            {
                stringBuilder.Append(GetNextWord());
                if (i < sentenceLength - 1)
                    stringBuilder.Append(' ');
            }
        }

        protected virtual string GetNextWord()
        {
            return _words[GetNextWordNumber(_words.Length - 1)];
        }

        protected abstract int GetNextSentenceLength();

        protected abstract int GetNextWordNumber(int maxWordNumber);
    }
}
