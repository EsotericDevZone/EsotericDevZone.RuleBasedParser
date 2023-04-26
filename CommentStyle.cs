using System.Collections.Generic;
using System.Linq;

namespace EsotericDevZone.RuleBasedParser
{
    public class CommentStyle
    {
        public string[] InlineComments { get; }
        public (string Begin, string End)[] BlockComments { get; }

        public CommentStyle(IEnumerable<string> inlineComments, IEnumerable<(string, string)> blockComments)
        {
            InlineComments = inlineComments.ToArray();
            BlockComments = blockComments.ToArray();
        }
    }
}
