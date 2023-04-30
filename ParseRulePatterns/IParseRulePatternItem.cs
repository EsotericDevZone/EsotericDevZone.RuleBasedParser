using System.Collections.Generic;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal interface IParseRulePatternItem
    {        
        IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position);
    }
}
