using Bogus;

namespace FileGenerate
{
    public class BogusStringFactory : RandomStringFactoryBase
    {
        private static readonly Faker _faker = new Faker();

        protected override int GetNextNumber(int maxNumber)
        {
            return _faker.Random.Number(0, maxNumber);
        }

        protected override int GetNextSentenceLength(int maxSentenceLength)
        {
            return _faker.Random.Number(1, maxSentenceLength);
        }

        protected override string GetNextWord()
        {
            return _faker.Company.CompanyName();
        }
    }
}
