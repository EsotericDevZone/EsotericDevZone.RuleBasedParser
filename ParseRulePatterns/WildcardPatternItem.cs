﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal class WildcardPatternItem : IParseRulePatternItem
    {
        private readonly string[] Values;        
        public IParseRulePatternItemMatch Match(Parser parser, List<Token> tokens, int position)
        {
            if (Values.Contains(tokens[position].Value))
                return new WildcardMatch(tokens[position], position);
            return new ParseError($"Unexpected token : {tokens[position].Value}", position);
        }

        public WildcardPatternItem(string[] values)
        {           
            Values = values.Select(s => s.Replace(@"\|", "|")).ToArray();
        }

        public override string ToString() => string.Join("|", Values.Select(v => v.Replace("|", @"\|")));
    }
}
