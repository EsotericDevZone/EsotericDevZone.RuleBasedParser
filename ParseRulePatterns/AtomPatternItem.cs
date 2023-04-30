using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class AtomPatternItem : IParseRulePatternItem
    {
        public string Name { get; }        
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position, int stack = 0)
        {
            try
            {
                var resObj = parser.GetAtomBuilder(Name)(tokens[position].Value);
                var result = new ParseResult(tokens[position], resObj);
                return new ParseRecord(Name, result, position, 1) { Similarity = 1 };
            }
            catch(Exception e)
            {
                return new ParseError(e.Message, position);
            }            
        }

        public AtomPatternItem(string name)
        {
            Name = name;            
        }

        public override string ToString() => Name;        
    }
}
