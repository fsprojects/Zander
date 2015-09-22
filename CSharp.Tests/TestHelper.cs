using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
