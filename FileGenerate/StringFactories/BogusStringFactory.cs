using Bogus;

namespace FileGenerate
{
    public class BogusStringFactory : IRandomStringFactory
    {
        private static readonly Faker<FileEntry> _faker = new Faker<FileEntry>()
            .StrictMode(true)
            .RuleFor(x => x.Number, x => x.Random.Number(0, 10000))
            .RuleFor(x => x.Name, x => x.Company.CompanyName());

        public string Create()
        {
            var entry = _faker.Generate();
            return $"{entry.Number}. {entry.Name}";
        }

        private class FileEntry
        {
            public int Number { get; set; }
            public string Name { get; set; }
        }
    }
}
