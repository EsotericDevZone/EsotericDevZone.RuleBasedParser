using System.Linq;
using System.Text.RegularExpressions;

namespace EsotericDevZone.RuleBasedParser.Helpers
{
    internal class CommentsStyleHelper
    {        
        private static string EscapeChars(string s)
        {
            return Regex.Escape(s).Replace("/", "\\/").Replace("-", "\\/");
        }

        internal static string GetBlockCommentsRegex((string Begin, string End)[] styles)
        {
            return string.Join("|", styles.Select(s => $"({EscapeChars(s.Begin)}.*?{EscapeChars(s.End)})"));
        }

        internal static string GetInlineCommentsRegex(string[] styles)
        {
            return string.Join("|", styles.Select(s => $"({EscapeChars(s)}.*$)"));
        }
    }
}