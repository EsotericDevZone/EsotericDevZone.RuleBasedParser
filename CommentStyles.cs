using EsotericDevZone.Core.Collections;

namespace EsotericDevZone.RuleBasedParser
{
    public static class CommentStyles
    {
        public static readonly CommentStyle CStyle = new CommentStyle
            (
                Collections.ListOf("//"),
                Collections.ListOf(("/*", "*/"))
            );

        public static readonly CommentStyle PascalStyle = new CommentStyle
            (
                Collections.ListOf("//"),
                Collections.ListOf(("/*", "*/"), ("(*", "*)"))
            );

        public static readonly CommentStyle PythonStyle = new CommentStyle
            (
                Collections.ListOf("#"),
                Collections.ListOf(("\"\"\"", "\"\"\""))
            );
    }
}
