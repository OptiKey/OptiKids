using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JuliusSweetland.OptiKids.Enums;
using JuliusSweetland.OptiKids.Properties;
using log4net;

namespace JuliusSweetland.OptiKids.Extensions
{
    public static class StringExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string ToPrintableString(this string input)
        {
            if (input == null) return null;

            var sb = new StringBuilder();

            foreach (var c in input)
            {
                sb.Append(c.ToPrintableString());
            }
            return sb.ToString();
        }

        public static string ToString(this List<string> strings, string nullValue)
        {
            string output = nullValue;

            if (strings != null)
            {
                output = string.Join(",", strings);
            }

            return output;
        }
    }
}