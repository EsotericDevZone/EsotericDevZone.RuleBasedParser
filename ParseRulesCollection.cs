using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser
{
    public class ParseRulesCollection
    {
        private Parser Parser;

        public ParseRulesCollection(Parser parser)
        {
            Parser = parser;
        }

        private List<ParseRule> ParseRules = new List<ParseRule>();

        internal IEnumerable<ParseRule> GetRulesByKey(string ruleKey)
            => ParseRules.Where(r => r.Key == ruleKey);

        public void RegisterRule(string key, string rule, Func<ParseResult[], Token[], ParseResult> buildMethod)
            => ParseRules.Add(new ParseRule(Parser, key, rule.Split(' '), buildMethod));

        public void RegisterRule(string key, string rule, Func<ParseResult[], ParseResult> buildMethod)
            => RegisterRule(key, rule, (results, _) => buildMethod(results));

        public void RegisterRule(string key, string rule, Func<ParseResult, ParseResult> buildMethod)
            => RegisterRule(key, rule, results => buildMethod(results[0]));

        public void RegisterRule(string key, string rule, Func<ParseResult, ParseResult, ParseResult> buildMethod)
            => RegisterRule(key, rule, results => buildMethod(results[0], results[1]));

        public void RegisterRule(string key, string rule, Func<ParseResult, ParseResult, Token, ParseResult> buildMethod)
            => RegisterRule(key, rule, (results, tokens) => buildMethod(results[0], results[1], tokens[0]));

        public void RegisterRule<T, R>(string key, string rule, Func<T, R> buildMethod)
            => RegisterRule(key, rule, results =>
            {
                if (!(results[0].Value is T argT))
                    throw new ArgumentException("Invalid parse argument");
                return new ParseResult(results[0].GeneratorToken, buildMethod(argT));
            });

        public void RegisterRuleValueBuild<T, R>(string key, string rule, Func<T, R> buildMethod, int generatorTokenId)
            => RegisterRule(key, rule, (results, tokens) =>
            {
                if (!(results[0].Value is T argT))
                    throw new ArgumentException("Invalid parse argument");
                return new ParseResult(tokens[generatorTokenId], buildMethod(argT));
            });

        public void RegisterRuleValueBuild<T1,T2, R>(string key, string rule, Func<T1, T2, R> buildMethod, int generatorTokenId)
            => RegisterRule(key, rule, (results, tokens) =>
            {
                if (!(results[0].Value is T1 argT1))
                    throw new ArgumentException("Invalid parse argument");
                if (!(results[1].Value is T2 argT2))
                    throw new ArgumentException("Invalid parse argument");
                return new ParseResult(tokens[generatorTokenId], buildMethod(argT1, argT2));
            });

        public void RegisterRuleValueBuild<T1, T2, R>(string key, string rule, Func<T1, T2, R> buildMethod)
            => RegisterRule(key, rule, (results, tokens) =>
            {
                if (!(results[0].Value is T1 argT1))
                    throw new ArgumentException("Invalid parse argument");
                if (!(results[1].Value is T2 argT2))
                    throw new ArgumentException("Invalid parse argument");
                return new ParseResult(null, buildMethod(argT1, argT2));
            });
    }
}
