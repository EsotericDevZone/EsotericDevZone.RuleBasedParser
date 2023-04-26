using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace EsotericDevZone.RuleBasedParser
{
    public class RuleBasedParser
    {
        public string RootRuleKey { get; set; }

        public ParseRulesCollection ParseRules { get; }

        public TokensSplitOptions TokensSplitOptions { get; set; } = new TokensSplitOptions();
        public CommentStyle CommentStyle { get; set; } = CommentStyles.CStyle;

        public RuleBasedParser()
        {
            ParseRules = new ParseRulesCollection(this);
        }

        public object Parse(string input)
            => LookFor(RootRuleKey, input.SplitToTokens(TokensSplitOptions, CommentStyle), 0)?.Value;

        private ParseRecord LookFor(ParseRule rule, List<Token> tokens, int pos)
        {
            Console.WriteLine($"Trying {rule}");
            var pattern = rule.ParsePattern;
            var records = new List<ParseRecord>();
            var selectorsMatches = new List<Token>();

            tokens.ForEach(Console.WriteLine);


            int originalPos = pos;
            foreach (var item in pattern)
            {
                Console.WriteLine("HERE");
                if (pos >= tokens.Count) return null;
                Console.WriteLine("w");
                try
                {                    
                    var match = item.Match(this, tokens, pos);
                    Console.WriteLine(match);

                    if (match is WildcardMatch wildcardMatch)
                    {
                        selectorsMatches.Add(wildcardMatch.Value);
                    }
                    else if (match is ParseRecord record)
                    {                        
                        records.Add(record);
                        AddToCache(record);
                    }
                    else if (match is IgnoreMatch)
                    {
                    }
                    else throw new NotImplementedException();

                    pos += match.TokensCount;
                }
                catch (ParseException ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
            }

            var value = rule.BuildMethod(
                records.Select(r => r.Value).ToArray(),
                selectorsMatches.ToArray()
                );
            var _rec = new ParseRecord(rule.Key, value, originalPos, pos - originalPos);
            AddToCache(_rec);
            return _rec;            
        }

        internal ParseRecord LookFor(string ruleKey, List<Token> tokens, int pos)
        {
            ParseRecord fromCache;
            if ((fromCache = GetFromCache(ruleKey, pos)) != null)
                return fromCache;

            foreach (var rule in ParseRules.GetRulesByKey(ruleKey)) 
            {
                var rec = LookFor(rule, tokens, pos);
                if (rec != null)
                {
                    AddToCache(rec);
                    return rec;
                }
            }
            return null;
        }


        #region Atoms
        private Dictionary<string, Func<string, object>> Atoms = new Dictionary<string, Func<string, object>>();

        internal bool IsAtom(string name) => Atoms.ContainsKey(name);
        internal Func<string, object> GetAtomBuilder(string name) => Atoms[name];

        public void RegisterAtom(string atomName, Func<string, object> buildMethod)
        {
            Atoms[atomName] = buildMethod;
        }
        #endregion

        #region ParseCache
        private HashSet<ParseRecord> ParseCache = new HashSet<ParseRecord>(new ParseRecordComparer());
        
        private void AddToCache(ParseRecord r)
        {
            if(!ParseCache.Contains(r))            
                ParseCache.Add(r);                        
        }

        private ParseRecord GetFromCache(string ruleKey, int pos)
            => ParseCache.Where(r => r.RuleKey == ruleKey && r.Position == pos).FirstOrDefault();
        #endregion

    }  
}
