using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class WildcardPatternItem : IParseRulePatternItem
    {
        private string[] Values;        
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position, int stack = 0)
        {
            if (Values.Contains(tokens[position].Value))
                return new WildcardMatch(tokens[position], position) { Similarity = 1 };
            return new ParseError($"Unexpected token : {tokens[position]}", position);
        }

        public WildcardPatternItem(string[] values)
        {           
            Values = values.Select(s => s.Replace(@"\|", "|")).ToArray();
        }

        public override string ToString() => string.Join("|", Values.Select(v => v.Replace("|", @"\|")));
    }
}
