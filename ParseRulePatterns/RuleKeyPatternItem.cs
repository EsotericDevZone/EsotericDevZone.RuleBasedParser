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

        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position)
        {
            try
            {
                var rec = parser.LookFor(RuleKey, tokens, position);
                return rec;
            }
            catch(ParseException e)
            {
                throw new ParseException((ParseException)e);
            }
            catch (Exception e)
            {
                throw new ParseException(tokens[position], e.Message);
            }
        }

        public RuleKeyPatternItem(string ruleKey)
        {
            RuleKey = ruleKey;
        }

        public override string ToString() => RuleKey;        
    }
}
