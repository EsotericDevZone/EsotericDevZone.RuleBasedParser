using EsotericDevZone.Core.Collections;

namespace EsotericDevZone.RuleBasedParser.Presets
{
    public static class CommentStyles
    {
        public static readonly CommentStyle NoCommentsStyle = new CommentStyle
            (
                Collections.EmptyList<string>(),
                Collections.EmptyList<(string, string)>()
            );

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
