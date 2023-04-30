using EsotericDevZone.RuleBasedParser.ParseRulePatterns;
namespace EsotericDevZone.RuleBasedParser
{
    internal class IgnoreMatch : IParseRulePatternItemMatch
    {
        public int Position { get; }

        public int TokensCount { get; } = 1;
        public double Similarity { get; set; }

        public IgnoreMatch(int position, int tokensCount=1)
        {
            Position = position;
            TokensCount = tokensCount;
        }
    }
}
