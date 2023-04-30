using System;
using System.Collections.Generic;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class LiteralPatternItem : IParseRulePatternItem
    {
        public string Name { get; }
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position)
        {
            if (tokens[position].Value != Name)
            {
                return new ParseError($"Expected '{Name}', got '{tokens[position].Value}'", position);
            }                
            return new IgnoreMatch(position);
        }

        public LiteralPatternItem(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;        
    }
}
