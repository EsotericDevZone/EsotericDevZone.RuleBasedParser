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
        public IParseRulePatternItemMatch Match(RuleBasedParser parser, List<Token> tokens, int position)
        {
            try
            {
                var resObj = parser.GetAtomBuilder(Name)(tokens[position].Value);
                var result = new ParseResult(tokens[position], resObj);
                return new ParseRecord(Name, result, position, 1);
            }
            catch(Exception e)
            {
                throw new ParseException(e);
            }            
        }

        public AtomPatternItem(string name)
        {
            Name = name;            
        }

        public override string ToString() => Name;        
    }
}
