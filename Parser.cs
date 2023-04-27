using EsotericDevZone.RuleBasedParser.ParseRulePatterns;
using EsotericDevZone.RuleBasedParser.Presets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EsotericDevZone.RuleBasedParser
{
    public class Parser
    {
        public string RootRuleKey { get; set; }

        public ParseRulesCollection ParseRules { get; }

        public TokensSplitOptions TokensSplitOptions { get; set; } = new TokensSplitOptions();
        public CommentStyle CommentStyle { get; set; } = CommentStyles.CStyle;

        public Parser()
        {
            ParseRules = new ParseRulesCollection(this);
        }        

        public Parser(TokensSplitOptions tokensSplitOptions, CommentStyle commentStyle) : this()
        {
            TokensSplitOptions = tokensSplitOptions;
            CommentStyle = commentStyle;
        }

        public object Parse(string input)
        {
            ParseCache.Clear();
            try
            {
                var tokens = input.SplitToTokens(TokensSplitOptions, CommentStyle);

                if(tokens.Count==0)
                {
                    throw new NoTokensProvidedException("No tokens provided");
                }

                //tokens.ForEach(Console.WriteLine);
                var result = LookFor(RootRuleKey, tokens, 0);
                if(result!=null)
                {                 
                    
                    if(result.Position!=0 || result.TokensCount!=tokens.Count)
                    {
                        throw new ParseException(tokens[0], "Insuficient tokens");
                    }
                    return result.Result.Value;
                }
            }          
            catch(ParseException ex)
            {
                throw new ParseException(ex);
            }
            throw new ParseException("Parse error: No result");
        }

        private ParseRecord LookFor(ParseRule rule, List<Token> tokens, int pos)
        {
            //Console.WriteLine($"{pos,5}. Looking for {rule}");

            var pattern = rule.ParsePattern;

            List<ParseRecord> finalRecords = null;
            List<Token> finalSelectorMatches = null;
            
            var records = new List<ParseRecord>();
            var selectorsMatches = new List<Token>();
            

            Exception raisedException = null;

            int tailReturn = -1;
            int originalPos = pos;
            int i = 0;
            for (i = 0; i < pattern.Length; i++) 
            {
                var item = pattern[i];                

                if (item is RepeatableTailItem) 
                {
                    tailReturn = i;

                    finalRecords = records.ToList();
                    finalSelectorMatches = selectorsMatches.ToList();                    
                    continue;
                }                
                
                try
                {
                    if (pos >= tokens.Count)
                    {
                        throw new ParseException(tokens.Last(), "Insufficient tokens");
                    }

                    var match = item.Match(this, tokens, pos);
                    //Console.WriteLine(match);

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
                    else throw new NotImplementedException("Invalid match");

                    pos += match.TokensCount;
                }
                catch (ParseException ex)
                {
                    raisedException = ex;                    
                    break;
                }

                if(i==pattern.Length-1)
                {
                    finalRecords = records.ToList();
                    finalSelectorMatches = selectorsMatches.ToList();
                    if (tailReturn >= 0)
                    {                                                                   
                        i = tailReturn;
                        raisedException = null;
                    }                    
                }                
            }

            if (finalRecords == null || (raisedException != null && i != tailReturn + 1)) 
            {
                throw raisedException;           
            }
            
            var value = rule.BuildMethod(
                finalRecords.Select(r => r.Result).ToArray(),
                finalSelectorMatches.ToArray()
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
            ParseException raisedException = null;            
            
            foreach (var rule in ParseRules.GetRulesByKey(ruleKey)) 
            {                
                try
                {
                    var rec = LookFor(rule, tokens, pos);
                    if (rec != null)
                    {                        
                        AddToCache(rec);
                        return rec;
                    }
                }
                catch(ParseException e)
                {
                    raisedException = new ParseException((ParseException)e);
                }
                catch (NullReferenceException e)
                {
                    raisedException = new ParseException(tokens[pos], e.Message);
                }
                catch (Exception e)
                {
                    raisedException = new ParseException(tokens[pos], e.Message);                    
                }                
            }
            

            if(raisedException!=null)
                throw new ParseException(raisedException);
            //throw new ParseException("Parse failed");            

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
