using EsotericDevZone.Core;
using EsotericDevZone.RuleBasedParser.ParseRulePatterns;
using EsotericDevZone.RuleBasedParser.Presets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

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
            var tokens = input.SplitToTokens(TokensSplitOptions, CommentStyle);
            tokens.ForEach(_ => Debug.WriteLine(_));

            if(tokens.Count==0)
            {
                throw new NoTokensProvidedException("No tokens provided");
            }

            var result = LookFor(RootRuleKey, tokens, 0);

            if (result == null)
                throw new ParseException("Parse result was null");

            if (result.Error != null)
                throw new ParseException(tokens[result.Error.Position], result.Error.ToString());

            if (result.Position != 0 || result.TokensCount != tokens.Count) 
            {
                throw new ParseException(tokens.Last(), "Insuficient tokens");
            }
            return result.Result.Value;                   
        }

        private ParseRecord LookFor(ParseRule rule, List<Token> tokens, int pos, int stack = 0)
        {
            var stackstr = Enumerable.Range(0, stack).Select(_ => "    ").JoinToString("");
            Debug.WriteLine($"{pos,5}. Looking for {rule}");

            var pattern = rule.ParsePattern;

            List<ParseRecord> finalRecords = null;
            List<Token> finalSelectorMatches = null;
            
            var records = new List<ParseRecord>();
            var selectorsMatches = new List<Token>();           
            
            int originalPos = pos;            

            var mandatoryPattern = pattern.TakeWhile(_ => !(_ is RepeatableTailItem)).ToArray();
            var optionalPattern = pattern.SkipWhile(_ => !(_ is RepeatableTailItem)).ToArray().Skip(1).ToArray();

            Debug.WriteLine($"Mandatory = {string.Join(" ",mandatoryPattern.ToList())}");
            Debug.WriteLine($"Optional = {string.Join(" ", optionalPattern.ToList())}");

            List<ParseError> encounteredErrors = new List<ParseError>();

            bool dealWithPatternItem(IParseRulePatternItem item)
            {
                if (pos >= tokens.Count)
                {
                    encounteredErrors.Add(new ParseError("Unexpected end of input", tokens.Count));
                    return false;                    
                }                

                var match = item.Match(this, tokens, pos);

                if (match is ParseError error) 
                {
                    encounteredErrors.Add(error);
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
                    encounteredErrors.Add(new ParseError("Unknown match type", 0));
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
                    Console.WriteLine("--0-----------------------------------");
                    Console.WriteLine(encounteredErrors.JoinToString("\n").Indent(stackstr));                    
                    var maxSim = encounteredErrors.Max(_ => _.Similarity);
                    var error = encounteredErrors
                        .Where(_ => _.Similarity == maxSim).First();
                    error = new ParseError(error, Math.Max(error.Similarity, 1.0 * solCount / patCount));
                    //Console.WriteLine(error);
                    return new ParseRecord(error);
                }
                else solCount++;

            encounteredErrors.Clear();

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
                            Console.WriteLine("--R-----------------------------------");
                            Console.WriteLine(encounteredErrors.JoinToString("\n").Indent(stackstr));
                            var maxSim = encounteredErrors.Max(_ => _.Similarity);
                            var error = encounteredErrors
                                .Where(_ => _.Similarity == maxSim).First();
                            pError = new ParseError(error, 1.0 * solCount / patCount);
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

        internal ParseRecord LookFor(string ruleKey, List<Token> tokens, int pos, int stack = 0)
        {
            var stackstr = Enumerable.Range(0, stack).Select(_ => "    ").JoinToString("");

            ParseRecord fromCache;
            if ((fromCache = GetFromCache(ruleKey, pos)) != null)
                return fromCache;

            var encounteredErrors = new List<ParseError>();
            
            foreach (var rule in ParseRules.GetRulesByKey(ruleKey)) 
            {
                var rec = LookFor(rule, tokens, pos, stack + 1);
                if (rec != null)
                {
                    if (rec.Error != null)
                    {
                        encounteredErrors.Add(rec.Error);
                        Console.WriteLine($"{stackstr}Recv: {rec.Error}");
                    }
                    else
                    {
                        AddToCache(rec);                        
                        return rec;
                    }
                }               
            }

            Console.WriteLine($"{stackstr}Final - {encounteredErrors.Count}");
            Console.WriteLine(encounteredErrors.JoinToString("\n").Indent(stackstr));

            var maxSim = encounteredErrors.Max(_ => _.Similarity);
            var error = encounteredErrors.Where(_ => _.Similarity == maxSim).First();
            return new ParseRecord(error);
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
