using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            var parsed = new BlockEx(@" _+ ""Report Title"" _+  @Time @Page : report_title")
                .Match(file_content.Take(1).ToArray());
            Assert.That(ToValueTuples(parsed), Is.EquivalentTo(ToTuples(
                new[] { new[] { "Time", "16/09/15 16:17" }, new[] { "Page", "Page: 1" } })));
        }

        [Test]
        public void Can_recognize_company()
        {
            var parsed = new BlockEx(@" ""Company AB"" _+          : company")
                .Match(file_content.Skip(1).Take(1).ToArray());
            Assert.That(ToValueTuples(parsed), Is.EquivalentTo(ToTuples(new string[0][])));
        }

        [Test]
        public void Can_recognize_text()
        {
            var parsed = new BlockEx(@" @Text      _ _ _ _ _ _                _ _ _ _  _          : text+")
                .Match(file_content.Skip(2).Take(2).ToArray());
            Assert.That(ToValueTuples(parsed), Is.EquivalentTo(ToTuples(new[] {
                       new[] { "Text", "Some text" },
                       new[] { "Text", "that goes on and explains the report" },
                })));
        }

        [Test]
        public void Can_recognize_header()
        {
            var parsed = new BlockEx(@" _         Id _  Value  Type _ _ ""Attribute 1"" _ ""Attribute 2"" _*  : header")
                .Match(file_content.Skip(4).Take(1).ToArray());
            Assert.That(ToValueTuples(parsed), Is.EquivalentTo(ToTuples(new string[0][])));
        }

        private const string type1 = @" _          _ _ _ _ _ ""Report Title"" _  _  _  @Time @Page : report_title
                                    ""Company AB"" _ _ _ _ _ _                _ _ _ _  _          : company
                                        @Text      _ _ _ _ _ _                _ _ _ _  _          : text+
                                        _         Id _  Value  Type _ _ ""Attribute 1"" _ ""Attribute 2"" _  _ : header
                                        _        @Id _ @Value @Type _ _ @Attribute1     _ @Attribute2     _  _ : row+
                    ";
        private string[][] expected_1 = new[] {
                    new[] { "Time", "16/09/15 16:17" }, new[] { "Page", "Page: 1" },
                    new[] { "Text", "Some text" },
                    new[] { "Text", "that goes on and explains the report" },
                    new[] {"Id", "44" },new[] {"Value", "XYZ" }, new[] {"Type", "A" },
                            new[] {"Attribute1", "" },new[] {"Attribute2", "" }
                };
        [Test]
        public void Can_extract_information_from_first_block()
        {
            var parsed = new BlockEx(type1)
                .Match(file_content.Take(6).ToArray());
            Assert.That(ToValueTuples(parsed), Is.EquivalentTo(ToTuples(
                expected_1)));
        }
        private const string type2 = @" _          _ _ _ _ _ ""Report Title"" _  _  _  @Time @Page : report_title
                                    ""Company AB"" _ _ _ _ _ _                _ _ _ _  _          : company
                                        @Text      _ _ _ _ _ _                _ _ _ _  _          : text+
                                        _         Id _   Value  Type   _ _ ""Attribute 1""  ""Attribute 2"" _  _  _ : header
                                        _         _  @Id _      @Value @Type  _ @Attribute1 @Attribute2     _  _  _ : row+ 
                    ";
        private string[][] expected_2 = new[] { new[] { "Time", "16/09/15 16:17" }, new[] { "Page", "Page: 2" },
                    new [] { "Text", "Some text" },
                    new [] { "Text", "that goes on and explains the report" },
                    new [] {"Id", "51" }, new [] {"Value", "XYZ" },new [] {"Type", "A" },
                            new [] {"Attribute1", "255" },new [] {"Attribute2", "" }
                };
        [Test]
        public void Can_extract_information_from_second_block()
        {
            var parsed = new BlockEx(type2)
                .Match(file_content.Skip(10).Take(6).ToArray());
            Assert.That(ToValueTuples(parsed), Is.EquivalentTo(
                ToTuples(expected_2)));
        }

        /// <summary>
        /// a procedural implementation of the obsolete "Builder.parse"
        /// Using while and goto to recreate the recursion.
        /// </summary>
        private IEnumerable<MatchBlock> Parse(IEnumerable<BlockEx> matchers, string[][] matrix)
        {
            int index = 0;
            while (index < matrix.Length)
            {
                foreach (var matcher in matchers)
                {
                    var m = matcher.Match(matrix.Skip(index).ToArray());
                    if (m.Success)
                    {
                        yield return m;
                        index = m.Size.Height + index;
                        goto matched;
                    }
                }
                throw new Exception("Could not match at index " + index);
                matched: { }
            }
        }

        [Test]
        public void Can_extract_entire_thing()
        {
            var parsed = Parse(new[] { new BlockEx(type1), new BlockEx(type2) },
                file_content).ToArray();
            Assert.That(parsed.Length, Is.EqualTo(3));
            var expected = new List<Tuple<string, string>>();
            expected.AddRange(ToTuples(expected_1));
            expected.AddRange(ToTuples(expected_2));
            var tuples = parsed.SelectMany(ToValueTuples).ToArray();
            //Assert.That(tuples.Take(expected.Count()), Is.EquivalentTo(expected));
        }

        [Test]
        public void Parse_a_simple_format()
        {
            var section = ParseCsv(@";H
D1;
D2;
;D3");

            var parsed = Parse(new[] {
                            new BlockEx(@"
                                _   ""H"" : header 
                                @V   _ : row+"),
                            new BlockEx(
                                "_   @V : row+")
            }, section).ToArray();
            Assert.That(parsed.SelectMany(ToValueTuples), Is.EquivalentTo(ToTuples(
                new[] { new[] { "V", "D1" }, new[] { "V", "D2" }, new[] { "V", "D3" } })));
        }

    }
}
