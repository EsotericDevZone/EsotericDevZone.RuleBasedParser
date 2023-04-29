using EsotericDevZone.Core.Collections;

namespace EsotericDevZone.RuleBasedParser.Presets
{
    public static class CommentStyles
    {
        public static readonly CommentStyle NoCommentsStyle = new CommentStyle
            (
                Lists.Empty<string>(),
                Lists.Empty<(string, string)>()
            );

        public static readonly CommentStyle CStyle = new CommentStyle
            (
                Lists.Of("//"),
                Lists.Of(("/*", "*/"))
            );

        public static readonly CommentStyle PascalStyle = new CommentStyle
            (
                Lists.Of("//"),
                Lists.Of(("/*", "*/"), ("(*", "*)"))
            );

        public static readonly CommentStyle PythonStyle = new CommentStyle
            (
                Lists.Of("#"),
                Lists.Of(("\"\"\"", "\"\"\""))
            );
    }
}
