using System;

namespace EsotericDevZone.RuleBasedParser.Presets
{
    public class ParseResultBuilders
    {
        public static ParseResult Self(ParseResult result) => result;
        public static ParseResult Null(ParseResult _) => null;

        public static Func<ParseResult[], Token[], ParseResult> LeftAssociate(Func<ParseResult, ParseResult, Token, ParseResult> op)
        {
            return (results, tokens) =>
            {
                ParseResult result = results[0];
                for (int i = 0; i < tokens.Length; i++)
                    result = op(result, results[i + 1], tokens[i]);
                return result;
            };
        }

        public static Func<ParseResult[], Token[], ParseResult> RightAssociate(Func<ParseResult, ParseResult, Token, ParseResult> op)
        {
            return (results, tokens) =>
            {
                ParseResult result = results[results.Length - 1];
                for (int i = tokens.Length - 1; i >= 0; i--)
                    result = op(results[i], result, tokens[i]);
                return result;
            };
        }
    }
}
