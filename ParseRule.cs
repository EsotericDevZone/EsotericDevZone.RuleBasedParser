using EsotericDevZone.RuleBasedParser.ParseRulePatterns;
using System;
using System.Linq;

namespace EsotericDevZone.RuleBasedParser
{
    internal class ParseRule
    {
        public string Key { get; }
        public IParseRulePatternItem[] ParsePattern { get; }

        public Func<ParseResult[], Token[], ParseResult> BuildMethod;

        public ParseRule(RuleBasedParser parser, string key, string[] parsePattern, Func<ParseResult[], Token[], ParseResult> buildMethod)
        {
            Key = key;
            ParsePattern = parsePattern.Select(item => ParseRulePatternItems.FromString(parser, item)).ToArray();
            BuildMethod = buildMethod;
        }

        public override string ToString()
            => $"ParseRule(Key={Key}, ParsePattern='{string.Join(" ", ParsePattern.Select(p => p.ToString()))}')";
    }    
}
