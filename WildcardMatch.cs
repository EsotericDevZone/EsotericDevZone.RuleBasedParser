using EsotericDevZone.RuleBasedParser.ParseRulePatterns;

namespace EsotericDevZone.RuleBasedParser
{
    internal class WildcardMatch : IParseRulePatternItemMatch
    {
        public Token Value { get; }
        public int Position { get; }
        public int TokensCount { get; } = 1;        

        public WildcardMatch(Token value, int position)
        {
            Value = value;
            Position = position;
        }
    }
}
