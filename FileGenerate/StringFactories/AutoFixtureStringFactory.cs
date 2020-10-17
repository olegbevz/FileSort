using AutoFixture;
using System.Text;

namespace FileGenerate
{
    public class AutoFixtureStringFactory : RandomStringFactoryBase
    {
        private readonly Fixture _fixture = new Fixture();        

        protected override int GetNextNumber(int maxNumber)
        {
            var number = _fixture.Create<int>();
            if (number > maxNumber)
                number = maxNumber;
            return number;
        }
        protected override void AppendNextName(StringBuilder stringBuilder)
        {
            stringBuilder.Append(_fixture.Create<string>());
        }

        protected override void AppendNextName(StringBuilder stringBuilder, int length)
        {
            var @string = _fixture.Create<string>();
            while (@string.Length < length)
                @string += _fixture.Create<string>();
            if (@string.Length > length)
                @string = @string.Substring(0, length);
            stringBuilder.Append(@string);
        }
    }
}
