using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zander;

namespace CSharp.Tests
{
    [TestFixture]
    public class BuilderTests
    {
        private Builder builder;

        [TestFixtureSetUp]
        public void OnceBeforeAnyTest()
        {
            builder = new Builder();
        }

        [Test]
        public void Test()
        {
            var section = new[] { new[] { "", "H" }, new[] { "D1", "" }, new[] { "D2", "" }, new[] { "", "D3" } };

            var result = builder.Block("fst",@"
                            _   @V : header 
                            @V   _ : data_rows+")
                   .Block("snd", @"
                            _   @V : data_rows2+
").Parse(section);

            Assert.That(result.ToArray(), Is.EquivalentTo(new ParsedBlock[] {
                new ParsedBlock( "fst",new []{
                        new ParsedRow("header", new[] { "H" }),
                        new ParsedRow("data_rows", new[] { "D1" }),
                        new ParsedRow("data_rows", new[] { "D2" }),
                }),
                new ParsedBlock("snd", new []{
                        new ParsedRow("data_rows2", new[] { "D3" }),
                })
            }));
        }
    }
}
