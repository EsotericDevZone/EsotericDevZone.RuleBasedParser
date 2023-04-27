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
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position)
        {
            if (tokens[position].Value != Name)
                throw new ParseException(tokens[position], $"Expected '{Name}', got '{tokens[position].Value}'");
            return new IgnoreMatch(position);
        }

        public LiteralPatternItem(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;        
    }
}
