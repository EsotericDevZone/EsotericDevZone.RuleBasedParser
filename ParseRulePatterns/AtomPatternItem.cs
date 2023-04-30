using System.Collections.Generic;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class AtomPatternItem : IParseRulePatternItem
    {
        public string Name { get; }        
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position)
        {            
            var atom = parser.GetAtomBuilder(Name)(tokens[position].Value);            
            if (atom.Failed) 
            {
                return new ParseError(atom.ErrorMessage, position);
            }            
            var result = new ParseResult(tokens[position], atom.Value);
            return new ParseRecord(Name, result, position, 1);      
        }

        public AtomPatternItem(string name)
        {
            Name = name;            
        }

        public override string ToString() => Name;        
    }
}
