using NUnit.Framework;
using System.IO;
using System.Linq;
using Zander;

namespace CSharp.Tests.RealWorld
{
    [TestFixture]
    public class Parse_slightly_complicated_format : TestHelper
    {
        private string[][] file_content;

        [TestFixtureSetUp]
        public void OnceBeforeAnyTest()
        {
            file_content = ParseCsv(File.ReadAllText(Path.Combine("RealWorld", "slightly_complicated_format.csv")));
        }

        [Test]
        public void Can_recognize_title()
        {
            var parsed = new ParserBuilder()
                .Block("only_title", @" _+ ""Report Title"" _+  @Time @Page : report_title")
                .Parse(file_content.Take(1).ToArray());
            Assert.That(parsed.ToArray(), Is.EquivalentTo(new ParsedBlock[] {
                new ParsedBlock( "only_title",new []{
                        new ParsedRow("report_title", new[] { Kv("Time", "16/09/15 16:17"), Kv("Page","Page: 1") }),
                })
            }));
        }

        [Test]
        public void Can_recognize_company()
        {
            var parsed = new ParserBuilder()
                .Block("only_company", @" ""Company AB"" _+          : company")
                .Parse(file_content.Skip(1).Take(1).ToArray());
            Assert.That(parsed.ToArray(), Is.EquivalentTo(new ParsedBlock[] {
                new ParsedBlock( "only_company", new [] { new ParsedRow("company", EmptyKvs()) })}));
        }

        [Test]
        public void Can_recognize_text()
        {
            var parsed = new ParserBuilder()
                .Block("only_text", @" @Text      _ _ _ _ _ _                _ _ _ _  _          : text+")
                .Parse(file_content.Skip(2).Take(2).ToArray());
            Assert.That(parsed.ToArray(), Is.EquivalentTo(new ParsedBlock[] {
                new ParsedBlock( "only_text", new [] {
                       new ParsedRow("text", new[] { Kv("Text", "Some text") }),
                       new ParsedRow("text", new[] { Kv("Text", "that goes on and explains the report") }),
                })}));
        }

        [Test]
        public void Can_recognize_header()
        {
            var parsed = new ParserBuilder()
                .Block("only_header", @" _         Id _  Value  Type _ _ ""Attribute 1"" _ ""Attribute 2"" _*  : header")
                .Parse(file_content.Skip(4).Take(1).ToArray());
            Assert.That(parsed.ToArray(), Is.EquivalentTo(new ParsedBlock[] {
                new ParsedBlock( "only_header", new [] {
                        new ParsedRow("header", EmptyKvs())
                })}));
        }

        private const string type1 = @" _          _ _ _ _ _ ""Report Title"" _  _  _  @Time @Page : report_title
                                    ""Company AB"" _ _ _ _ _ _                _ _ _ _  _          : company
                                        @Text      _ _ _ _ _ _                _ _ _ _  _          : text+
                                        _         Id _  Value  Type _ _ ""Attribute 1"" _ ""Attribute 2"" _  _ : header
                                        _        @Id _ @Value @Type _ _ @Attribute1     _ @Attribute2     _  _ : row+
                    ";

        [Test]
        public void Can_extract_information_from_first_block()
        {
            var parsed = new ParserBuilder()
                .Block("type1", type1)
                .Parse(file_content.Take(6).ToArray());
            Assert.That(parsed.ToArray(), Is.EquivalentTo(new ParsedBlock[] {
                new ParsedBlock( "type1",new []{
                        new ParsedRow("report_title", new[] { Kv("Time", "16/09/15 16:17"), Kv("Page","Page: 1") }),
                        new ParsedRow("company", EmptyKvs()),
                        new ParsedRow("text", new[] { Kv("Text", "Some text") }),
                        new ParsedRow("text", new[] { Kv("Text", "that goes on and explains the report") }),
                        new ParsedRow("header", EmptyKvs()),
                        new ParsedRow("row", new[] { Kv("Id", "44"), Kv("Value", "XYZ"), Kv("Type", "A"),
                            Kv("Attribute1", ""),Kv("Attribute2", ""), }),
                })
            }));
        }
        private const string type2 = @" _          _ _ _ _ _ ""Report Title"" _  _  _  @Time @Page : report_title
                                    ""Company AB"" _ _ _ _ _ _                _ _ _ _  _          : company
                                        @Text      _ _ _ _ _ _                _ _ _ _  _          : text+
                                        _         Id _   Value  Type   _ _ ""Attribute 1""  ""Attribute 2"" _  _  _ : header
                                        _         _  @Id _      @Value @Type  _ @Attribute1 @Attribute2     _  _  _ : row+ 
                    ";

        [Test]
        public void Can_extract_information_from_second_block()
        {
            var parsed = new ParserBuilder()
                .Block("type2", type2)
                .Parse(file_content.Skip(10).Take(6).ToArray());
            Assert.That(parsed.ToArray(), Is.EquivalentTo(new ParsedBlock[] {
                new ParsedBlock( "type2",new []{
                        new ParsedRow("report_title", new[] { Kv("Time", "16/09/15 16:17"), Kv("Page","Page: 2") }),
                        new ParsedRow("company", EmptyKvs()),
                        new ParsedRow("text", new[] { Kv("Text", "Some text") }),
                        new ParsedRow("text", new[] { Kv("Text", "that goes on and explains the report") }),
                        new ParsedRow("header", EmptyKvs()),
                        new ParsedRow("row", new[] { Kv("Id", "51"), Kv("Value", "XYZ"), Kv("Type", "A"),
                            Kv("Attribute1", "255"),Kv("Attribute2", ""), }),
                })
            }));
        }

        [Test]
        public void Can_extract_entire_thing()
        {
            var parsed = new ParserBuilder()
                .Block("type1", type1)
                .Block("type2", type2)
                .Parse(file_content).ToArray();
        }
    }
}
