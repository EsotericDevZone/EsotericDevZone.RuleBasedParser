using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class RuleKeyPatternItem : IParseRulePatternItem
    {
        public string RuleKey { get; }

        public IParseRulePatternItemMatch Match(RuleBasedParser parser, List<Token> tokens, int position)
        {
            var rec = parser.LookFor(RuleKey, tokens, position);
            if (rec == null)
                throw new ParseException("Parse failed");
            return rec;
        }

        public RuleKeyPatternItem(string ruleKey)
        {
            RuleKey = ruleKey;
        }
    }
}
