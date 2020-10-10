using AutoFixture;
using System.Text;

namespace FileGenerate
{
    public class AutoFixtureStringFactory : RandomStringFactory
    {
        private readonly Fixture _fixture = new Fixture();

        protected override int GetNextNumber()
        {
            return _fixture.Create<int>();
        }

        protected override void AppendNextName(StringBuilder stringBuilder)
        {
            stringBuilder.Append(_fixture.Create<string>());
        }
    }
}
