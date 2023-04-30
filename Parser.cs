using EsotericDevZone.Core;
using EsotericDevZone.RuleBasedParser.ParseRulePatterns;
using EsotericDevZone.RuleBasedParser.Presets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EsotericDevZone.RuleBasedParser
{
    /// <summary>
    /// Rule based parser tool
    /// </summary>
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

        /// <summary>
        /// Builder function to call when no tokens are provided (default: throws NoTokensProvidedException)
        /// </summary>
        public Func<ParseResult> EmptyBodyResult { get; set; } = () => throw new NoTokensProvidedException("No tokens provided");
        
        /// <summary>
        /// Parses an input sequence and returns the evaluated value according to the parsing rules
        /// </summary>        
        /// <exception cref="ParseException">A parse exception is thrown when a syntax error is encountered</exception>
        public object Parse(string input)
        {
            ParseCache.Clear();            
            var tokens = input.SplitToTokens(TokensSplitOptions, CommentStyle);            

            if(tokens.Count==0)
            {
                return EmptyBodyResult();                
            }

            var result = LookFor(RootRuleKey, tokens, 0);

            if (result == null)
                throw new ParseException("Parse result was null");

            if (result.Error != null)
            {                
                throw new ParseException(tokens[result.Error.Position], result.Error.Message);
            }

            if (result.Position != 0 || result.TokensCount != tokens.Count) 
            {
                throw new ParseException(tokens.Last(), "Insuficient tokens");
            }
            return result.Result.Value;                   
        }

        public T Parse<T>(string input)
        {
            var result = Parse(input);

            if (!(result is T))
                throw new InvalidCastException();

            return (T)result;
        }

        private ParseRecord LookFor(ParseRule rule, List<Token> tokens, int pos)
        {            
            var pattern = rule.ParsePattern;

            List<ParseRecord> finalRecords = null;
            List<Token> finalSelectorMatches = null;
            
            var records = new List<ParseRecord>();
            var selectorsMatches = new List<Token>();
            
            int originalPos = pos;            

            var mandatoryPattern = pattern.TakeWhile(_ => !(_ is RepeatableTailItem)).ToArray();
            var optionalPattern = pattern.SkipWhile(_ => !(_ is RepeatableTailItem)).ToArray().Skip(1).ToArray();            
            
            ParseError encounteredError = null;

            bool dealWithPatternItem(IParseRulePatternItem item)
            {
                if (pos >= tokens.Count)
                {
                    KeepMostRelevantError(ref encounteredError, new ParseError("Unexpected end of input", tokens.Count));                    
                    return false;                    
                }                

                var match = item.Match(this, tokens, pos);

                if (match is ParseError error) 
                {
                    KeepMostRelevantError(ref encounteredError, error);                    
                    return false;
                }

                if (match is WildcardMatch wildcardMatch)
                {
                    selectorsMatches.Add(wildcardMatch.Value);
                }
                else if (match is ParseRecord record)
                {
                    records.Add(record);
                    AddToCache(record);
                }
                else if (match is IgnoreMatch) { }
                else
                {
                    KeepMostRelevantError(ref encounteredError, new ParseError("Unknown match type", 0));                    
                    return false;
                }                
                pos += match.TokensCount;
                return true;
            }

            int patCount = mandatoryPattern.Length;
            int solCount = 0;

            foreach (var item in mandatoryPattern)
                if (!dealWithPatternItem(item))
                {
                    encounteredError.Relevance = Math.Max(encounteredError.Relevance, 1.0 * solCount / patCount);                 
                    return new ParseRecord(encounteredError);
                }
                else solCount++;

            encounteredError = null;            

            finalRecords = records.ToList();
            finalSelectorMatches = selectorsMatches.ToList();

            int posBackup = pos;
            if (optionalPattern.Length>0 && pos < tokens.Count)
            {
                ParseError pError = null;                
                while (pError == null)
                {
                    posBackup = pos;
                    patCount += optionalPattern.Length;
                    foreach (var item in optionalPattern)
                        if (!dealWithPatternItem(item))
                        {                            
                            encounteredError.Relevance = Math.Max(encounteredError.Relevance, 1.0 * solCount / patCount);
                            pError = encounteredError;
                            break;
                        }
                        else solCount++;
                    if (pError == null)
                    {
                        finalRecords = records.ToList();
                        finalSelectorMatches = selectorsMatches.ToList();
                    }
                }
                pos = posBackup;
            }
            
            var value = rule.BuildMethod(
                finalRecords.Select(r => r.Result).ToArray(),
                finalSelectorMatches.ToArray()
                );
            var _rec = new ParseRecord(rule.Key, value, originalPos, pos - originalPos);
            AddToCache(_rec);            
            return _rec;            
        }

        private void KeepMostRelevantError(ref ParseError target, ParseError next)
        {            
            if (target == null)
                target = next;
            else
                target = target.Relevance > next.Relevance ? target : next;
        }

        internal ParseRecord LookFor(string ruleKey, List<Token> tokens, int pos)
        {            
            ParseRecord fromCache;
            if ((fromCache = GetFromCache(ruleKey, pos)) != null)
                return fromCache;

            ParseError encounteredError = null;            
            
            foreach (var rule in ParseRules.GetRulesByKey(ruleKey)) 
            {
                var rec = LookFor(rule, tokens, pos);
                if (rec != null)
                {
                    if (rec.Error != null)
                    {
                        KeepMostRelevantError(ref encounteredError, rec.Error);                        
                    }
                    else
                    {
                        AddToCache(rec);                        
                        return rec;
                    }
                }               
            }
            
            return new ParseRecord(encounteredError);
        }


        #region Atoms
        private Dictionary<string, Func<string, AtomResult>> Atoms = new Dictionary<string, Func<string, AtomResult>>();

        internal bool IsAtom(string name) => Atoms.ContainsKey(name);
        internal Func<string, AtomResult> GetAtomBuilder(string name) => Atoms[name];

        public void RegisterAtom(string atomName, Func<string, AtomResult> buildMethod)
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
