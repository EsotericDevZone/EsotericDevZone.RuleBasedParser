using EsotericDevZone.RuleBasedParser.ParseRulePatterns;
using System;
using System.Collections.Generic;

namespace EsotericDevZone.RuleBasedParser
{
    class ParseRecord : IParseRulePatternItemMatch
    {
        public string RuleKey { get; }
        public ParseResult Result { get; }
        public int Position { get; }
        public int TokensCount { get; }        
        public ParseError Error { get; }

        public ParseRecord(ParseError error)
        {
            Error = error;
        }

        public ParseRecord(string ruleKey, ParseResult value, int position, int tokensCount)
        {
            RuleKey = ruleKey;
            Result = value;
            Position = position;
            TokensCount = tokensCount;
            Error = null;
        }

        public override string ToString() => $"Record(Key='{RuleKey}', Position={Position}, TokensCount={TokensCount})";       
    }

    internal class ParseRecordComparer : IEqualityComparer<ParseRecord>
    {
        public bool Equals(ParseRecord x, ParseRecord y)
        {
            return x.RuleKey == y.RuleKey && x.Position == y.Position;
        }

        public int GetHashCode(ParseRecord obj)
        {
            unchecked
            {
                return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.RuleKey)
                    + obj.Position.GetHashCode();
            }

        }
    }
}
