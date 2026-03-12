using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Theory]
        [InlineData("hhelo", "thang")]
        public void SimpleAdditionTest(string a, string b)
        {
            Assert.Equal(a + b, "hhelo tthang");
        }
    }
}