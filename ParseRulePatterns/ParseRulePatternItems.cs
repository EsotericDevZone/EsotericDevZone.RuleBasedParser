using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal static class ParseRulePatternItems
    {
        public static IParseRulePatternItem FromString(RuleBasedParser parser, string item)
        {
            if(item.ConstainsUnescapedVerticalLine())
            {
                var wildcards = Regex.Split(item, UnescapedVerticalLineRegex);
                return new WildcardPatternItem(wildcards);
            }
            if(item.StartsWith("@"))
            {
                return new RuleKeyPatternItem(item);
            }
            if(parser.IsAtom(item))
            {
                return new AtomPatternItem(item);
            }

            return null;
        }

        private static bool ConstainsUnescapedVerticalLine(this string str)
            => Regex.Matches(str, UnescapedVerticalLineRegex).Count > 0;

        private static readonly string UnescapedVerticalLineRegex = @"(?<!\\)\|";
    }
}
