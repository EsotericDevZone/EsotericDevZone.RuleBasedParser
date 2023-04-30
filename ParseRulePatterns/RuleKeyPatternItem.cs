using System.Collections.Generic;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class RuleKeyPatternItem : IParseRulePatternItem
    {
        public string RuleKey { get; }

        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position, int stack = 0)
        {
            var rec = parser.LookFor(RuleKey, tokens, position, stack + 1);
            if (rec.Error != null)
            {
                return new ParseError(rec.Error, rec.Error.Similarity);
            }
            return rec;
        }

        public RuleKeyPatternItem(string ruleKey)
        {
            RuleKey = ruleKey;
        }

        public override string ToString() => RuleKey;        
    }
}
