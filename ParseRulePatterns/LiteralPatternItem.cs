using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class LiteralPatternItem : IParseRulePatternItem
    {
        public string Name { get; }
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position, int stack = 0)
        {
            if (tokens[position].Value != Name)
            {
                return new ParseError($"Expected '{Name}', got '{tokens[position].Value}'", position);
            }                
            return new IgnoreMatch(position) { Similarity = 1 };
        }

        public LiteralPatternItem(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;        
    }
}
