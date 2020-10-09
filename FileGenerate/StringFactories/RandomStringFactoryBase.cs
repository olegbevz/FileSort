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

        public string Create()
        {
            var number = GetNextNumber();
            var name = GetNextName();

            return $"{number}. {name}";
        }

        protected abstract int GetNextNumber();

        protected virtual string GetNextName()
        {
            int sentenceLength = GetNextSentenceLength();

            var stringBuilder = new StringBuilder();
            for (int i = 0; i < sentenceLength; i++)
            {
                stringBuilder.Append(GetNextWord());
                if (i < sentenceLength - 1)
                    stringBuilder.Append(" ");
            }

            return stringBuilder.ToString();
        }

        protected virtual string GetNextWord()
        {
            return _words[GetNextWordNumber(_words.Length - 1)];
        }

        protected abstract int GetNextSentenceLength();

        protected abstract int GetNextWordNumber(int maxWordNumber);
    }
}
