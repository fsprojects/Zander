using NUnit.Framework;
using Zander;

namespace CSharp.Tests
{
    [TestFixture]
    public class PublicApiTests : TestHelper
    {
        [Test]
        public void Can_parse_a_simple_specified_format()
        {
            var section = ParseCsv(@";H
D1;
D2;
;D3");

            var blockEx = new BlockEx(@"
                            _   ""H"" : header 
                            @V   _ : row+
                            _   @V : row+");
            var m = blockEx.Match(section);
            Assert.That(ToValueTuples(m), Is.EquivalentTo(ToTuples(
                new[] { new[] { "V", "D1" }, new[] { "V", "D2" }, new[] { "V", "D3" } })));
            Assert.That(ToDictionaries(m), Is.EquivalentTo(new[] {
                ToDictionary(EmptyKvs()),
                ToDictionary(new[] { Kv("V", "D1") }),
                ToDictionary(new[] { Kv("V", "D2") }),
                ToDictionary(new[] { Kv("V", "D3") })
            }));
        }

    }
}
