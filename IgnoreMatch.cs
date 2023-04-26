using EsotericDevZone.RuleBasedParser.ParseRulePatterns;
namespace EsotericDevZone.RuleBasedParser
{
    internal class IgnoreMatch : IParseRulePatternItemMatch
    {
        public int Position { get; }

        public int TokensCount { get; } = 1;

        public IgnoreMatch(int position)
        {
            Position = position;
        }
    }
}
