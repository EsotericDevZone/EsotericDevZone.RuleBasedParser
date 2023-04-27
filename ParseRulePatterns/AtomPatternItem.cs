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
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position)
        {
            try
            {
                var resObj = parser.GetAtomBuilder(Name)(tokens[position].Value);
                var result = new ParseResult(tokens[position], resObj);
                return new ParseRecord(Name, result, position, 1);
            }
            catch(ParseException e)
            {
                throw new ParseException((ParseException)e);
            }
            catch(Exception e)
            {
                throw new ParseException(tokens[position], e.Message);
            }            
        }

        public AtomPatternItem(string name)
        {
            Name = name;            
        }

        public override string ToString() => Name;        
    }
}
