using System;
using System.Collections.Generic;
using System.Linq;
using Zander;
namespace CSharp.Tests
{
    public class TestHelper
    {
        public static KeyValuePair<string, string> Kv(string k, string v) { return new KeyValuePair<string, string>(k, v); }
        public static KeyValuePair<string, string>[] EmptyKvs()
        {
            return new KeyValuePair<string, string>[0];
        }
        /// <summary>
        /// this is not a proper csv parser...
        /// only intended to be used in tests
        /// </summary>
        /// <returns></returns>
        public static string[][] ParseCsv(string input)
        {
            return input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Split(new[] { ';' }))
                        .ToArray();
        }

        public static IEnumerable<Tuple<string, string>> ToValueTuples(MatchBlock m)
        {
            return m.Rows.SelectMany(r => r.Cells
                                .Where(c => c.CellType == CellType.Value)
                                .Select(c => Tuple.Create(c.Name, c.Value)));
        }

        public static IEnumerable<Tuple<string, string>> ToTuples(string[][] expected)
        {
            return expected.Select(kv => Tuple.Create(kv[0], kv[1]));
        }
        public static IEnumerable<IDictionary<string, string>> ToDictionaries(MatchBlock m)
        {
            return m.Rows.Select(r => r.ToDictionary());
        }
        public IDictionary<string,string> ToDictionary(KeyValuePair<string, string>[] v)
        {
            return v.ToDictionary(kv=>kv.Key,kv=>kv.Value);
        }
        public static string ToCsv(string[][] matrix)
        {
            return string.Join(Environment.NewLine, matrix.Select(row => string.Join(", ", row)).ToArray());
        }

    }
}
