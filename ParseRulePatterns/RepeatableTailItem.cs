using System.Collections.Generic;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class RepeatableTailItem : IParseRulePatternItem
    {
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position)
        {
            return new IgnoreMatch(position, 0);
        }

        public override string ToString() => "??";
    }
}