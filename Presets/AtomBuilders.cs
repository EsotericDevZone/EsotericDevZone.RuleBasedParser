using EsotericDevZone.Core;
using System;
using System.Text.RegularExpressions;

namespace EsotericDevZone.RuleBasedParser.Presets
{
    public static class AtomBuilders
    {
        public static AtomResult Integer(string input)
        {
            if (int.TryParse(input, out int value))
                return AtomResult.Atom(value);
            return AtomResult.Error($"Input is not an integer : '{input}'");            
        }
        public static AtomResult Real(string input)
        {
            if (double.TryParse(input, out double value)) 
                return AtomResult.Atom(value);
            return AtomResult.Error($"Input is not a real number : '{input}'");
        }        
        public static AtomResult Number(string input)
        {
            if (double.TryParse(input, out double value))
                return AtomResult.Atom(value);
            if (int.TryParse(input, out int ivalue))
                return AtomResult.Atom(ivalue);
            return AtomResult.Error($"Input is not a number : '{input}'"); 
        }

        public static AtomResult GenericString(string input, char startDelimiter, char endDelimiter)
        {
            if (input.Length < 2)
                return AtomResult.Error("Invalid string");
            if (!(input.StartsWith(startDelimiter.ToString()) && input.EndsWith(endDelimiter.ToString())))
                return AtomResult.Error("Invalid string");
            return AtomResult.Atom(input.FromLiteral());
        }

        public static AtomResult DoubleQuotedString(string input) => GenericString(input, '"', '"');
        public static AtomResult SingleQuotedString(string input) => GenericString(input, '\'', '\'');

        public static AtomResult Symbol(string input)
        {
            if (!Regex.IsMatch(input, @"^[_A-Za-z][_A-Za-z0-9]*$"))
                return AtomResult.Error("Invalid symbol");
            return AtomResult.Atom(input);
        }        
    }
}
