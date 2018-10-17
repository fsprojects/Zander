﻿using System.IO;
using System.Linq;
using Zander;
using Xunit;
namespace CSharp.Tests.RealWorld
{
    public class First_section : TestHelper
    {
        private string[][] file_content;

        public First_section()
        {
            file_content = ParseCsv(File.ReadAllText(Path.Combine("RealWorld", "slightly_complicated_format.csv")));
        }
        public static string A_Report_Title = @" _+ ""Report Title"" _+  @Time @Page : report_title";
        [Fact]
        public void Can_recognize_title()
        {
            var parsed = new BlockEx(A_Report_Title)
                .Match(file_content.Take(1).ToArray());
            Assert.Equal(ToTuples(new[] { new[] { "Time", "16/09/15 16:17" }, new[] { "Page", "Page: 1" } }),
                         ToValueTuples(parsed));
        }
        public static string B_Company_AB = @" ""Company AB"" _+          : company";
        [Fact]
        public void Can_recognize_company()
        {
            var parsed = new BlockEx(B_Company_AB)
                .Match(file_content.Skip(1).Take(1).ToArray());
            Assert.Equal(ToTuples(new string[0][]), ToValueTuples(parsed));
        }
        public static string C_Text_empty = @" @Text      _+ : text+";

        [Fact]
        public void Can_recognize_text()
        {
            var parsed = new BlockEx(C_Text_empty)
                .Match(file_content.Skip(2).Take(2).ToArray());
            Assert.Equal(ToTuples(new[] {
                       new[] { "Text", "Some text" },
                       new[] { "Text", "that goes on and explains the report" },
                }),ToValueTuples(parsed));
        }
        public static string D_header = @" _         Id _  Value  Type _ _ ""Attribute 1"" _ ""Attribute 2"" _*  : header";
        [Fact]
        public void Can_recognize_header()
        {
            var parsed = new BlockEx(D_header)
                .Match(file_content.Skip(4).Take(1).ToArray());
            Assert.Equal(ToTuples(new string[0][]),ToValueTuples(parsed));
        }
        public static string E_row = @" _        @Id _ @Value @Type _ _ (@Attribute1|_) _ (@Attribute2|_)     _+ : row+";
        [Fact]
        public void Can_recognize_row()
        {
            var parsed = new BlockEx(E_row)
                .Match(file_content.Skip(5).Take(1).ToArray());
            Assert.Equal(ToTuples(new[] {
                       new[] { "Id", "44" },
                       new[] { "Value", "XYZ" },
                       new[] { "Type", "A" },
                }),ToValueTuples(parsed));
            var parsed2 = new BlockEx(E_row)
                .Match(file_content.Skip(6).Take(1).ToArray());
            Assert.Equal(ToTuples(new[] {
                       new[] { "Id", "44" },
                       new[] { "Value", "XYZ" },
                       new[] { "Type", "B" },
                       new[] { "Attribute1", "255" },
                       new[] { "Attribute2", "155" }
                }),ToValueTuples(parsed2));

        }

        public static string Full()
        {
            return string.Join("\n", new[] { A_Report_Title, B_Company_AB, C_Text_empty, D_header, E_row });
        }
    }
}
