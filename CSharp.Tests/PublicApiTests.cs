using System.Linq;
using Xunit;
using Zander;

namespace CSharp.Tests
{
    public class PublicApiTests : TestHelper
    {
        [Fact]
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
            Assert.Equal(ToTuples(
                new[] { new[] { "V", "D1" }, new[] { "V", "D2" }, new[] { "V", "D3" } }),
                ToValueTuples(m));
            Assert.Equal(new[] {
                ToDictionary(EmptyKvs()),
                ToDictionary(new[] { Kv("V", "D1") }),
                ToDictionary(new[] { Kv("V", "D2") }),
                ToDictionary(new[] { Kv("V", "D3") })
            },ToDictionaries(m));
        }

        [Fact]
        public void Can_parse_several_matches()
        {
            var section = ParseCsv(@";H
D1;
D2;
;D3
;
;H
D4;
D5;
;D6
;");

            var blockEx = new BlockEx(@"
                            _   ""H"" : header 
                            @V   _ : row+
                            _   @V : row+
                            _   _");
            var ms = blockEx.Matches(section);
            Assert.Equal(2, ms.Length);
            Assert.Equal(ToTuples(new[] { new[] { "V", "D1" }, new[] { "V", "D2" }, new[] { "V", "D3" } }),
                         ToValueTuples(ms[0]));

            Assert.Equal(ToTuples(new[] { new[] { "V", "D4" }, new[] { "V", "D5" }, new[] { "V", "D6" } }),
                         ToValueTuples(ms[1]));
        }
    }
}
