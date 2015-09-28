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

        private readonly string type1 = First_section.Full();
        private string[][] expected_1 = new[] {
                    new[] { "Time", "16/09/15 16:17" }, new[] { "Page", "Page: 1" },
                    new[] { "Text", "Some text" },
                    new[] { "Text", "that goes on and explains the report" },
                    new[] {"Id", "44" },new[] {"Value", "XYZ" }, new[] {"Type", "A" },
                };
        [Test]
        public void Can_extract_information_from_first_block()
        {
            var parsed = new BlockEx(type1)
                .Match(file_content.Take(6).ToArray());
            Assert.That(ToValueTuples(parsed), Is.EquivalentTo(ToTuples(
                expected_1)));
        }
        private readonly string type2 = Second_section.Full();
        private string[][] expected_2 = new[] { new[] { "Time", "16/09/15 16:17" }, new[] { "Page", "Page: 2" },
                    new [] { "Text", "Some text" },
                    new [] { "Text", "that goes on and explains the report" },
                    new [] {"Id", "51" }, new [] {"Value", "XYZ" },new [] {"Type", "A" },
                            new [] {"Attribute1", "255" },
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
                var skip = Math.Min(0, index - 2);
                throw new Exception("Could not match at index " + index+" :\n\n "+
                    ToCsv(matrix.Skip(skip).Take(skip).ToArray())+"\n"+
                    ">> "+ToCsv(matrix.Skip(skip).Take(3).ToArray())
                    );
                matched: { }
            }
        }

        [Test]
        public void Can_extract_entire_thing()
        {
            var header = new BlockEx(string.Join(Environment.NewLine, new[] {
                First_section.A_Report_Title,
                First_section.B_Company_AB,
                First_section.C_Text_empty
            }));
            var split = header.Split(file_content)
                .Where(arr=>arr.Any())
                .ToArray();
            Assert.That(split.Length, Is.EqualTo(3));
            var types = new[] {
                new BlockEx(string.Join(Environment.NewLine, new[] {
                    First_section.D_header, First_section.E_row
                })), new BlockEx(string.Join(Environment.NewLine, new[] {
                    Second_section.D_header, Second_section.E_row
                }))
            };
            var parsed = split.SelectMany(block =>
                Parse(types, block)
            ).ToArray();
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
