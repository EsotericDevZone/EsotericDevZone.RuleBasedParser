using System;
using System.Text.RegularExpressions;

namespace EsotericDevZone.RuleBasedParser.Presets
{
    public static class AtomBuilders
    {
        public static object Integer(string input) => int.Parse(input);
        public static object Real(string input) => double.Parse(input);
        public static object Number(string input)
        {
            try
            {
                return Real(input);
            }
            catch (Exception)
            {
                return Integer(input);
            }
        }

        public static object GenericString(string input, char startDelimiter, char endDelimiter)
        {
            if (input.Length < 2)
                throw new ParseException("Invalid string");
            if (!(input.StartsWith(startDelimiter.ToString()) && input.EndsWith(endDelimiter.ToString())))
                throw new ParseException("Invalid string");
            return input.Substring(1, input.Length - 2).FromLiteral();
        }

        public static object DoubleQuotedString(string input) => GenericString(input, '"', '"');
        public static object SingleQuotedString(string input) => GenericString(input, '\'', '\'');

        public static object Symbol(string input)
        {
            if (!Regex.IsMatch(input, @"^[_A-Za-z][_A-Za-z0-9]*$"))
                throw new ParseException("Invalid symbol");
            return input;
        }        
    }
}
