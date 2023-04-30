using System.Collections.Generic;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class RepeatableTailItem : IParseRulePatternItem
    {
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position, int stack = 0)
        {
            return new IgnoreMatch(position, 0) { Similarity = 1 };
        }

        public override string ToString() => "??";
    }
}