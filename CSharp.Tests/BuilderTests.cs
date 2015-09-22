using NUnit.Framework;
using System.Linq;
using Zander;

namespace CSharp.Tests
{
    [TestFixture]
    public class BuilderTests: TestHelper
    {
        [Test]
        public void Can_parse_a_simple_specified_format()
        {
            var section = ParseCsv(@";H
D1;
D2;
;D3");

            var result = new ParserBuilder()
                   .Block("fst", @"
                            _   @V : header 
                            @V   _ : data_rows+")
                   .Block("snd", @"
                            _   @V : data_rows2+
                    ").Parse(section);

            Assert.That(result.ToArray(), Is.EquivalentTo(new ParsedBlock[] {
                new ParsedBlock( "fst",new []{
                        new ParsedRow("header", new[] { Kv("V","H") }),
                        new ParsedRow("data_rows", new[] { Kv("V", "D1") }),
                        new ParsedRow("data_rows", new[] { Kv("V", "D2") }),
                }),
                new ParsedBlock("snd", new []{
                        new ParsedRow("data_rows2", new[] { Kv("V", "D3") }),
                })
            }));
        }
    }
}
