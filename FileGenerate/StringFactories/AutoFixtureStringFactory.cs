using AutoFixture;

namespace FileGenerate
{
    public class AutoFixtureStringFactory : RandomStringFactory
    {
        private readonly Fixture _fixture = new Fixture();

        protected override int GetNextNumber()
        {
            return _fixture.Create<int>();
        }

        protected override string GetNextName()
        {
            return _fixture.Create<string>();
        }
    }
}
