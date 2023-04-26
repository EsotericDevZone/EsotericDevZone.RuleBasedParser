namespace EsotericDevZone.RuleBasedParser
{
    public class ParseResultBuilders
    {
        public static ParseResult Self(ParseResult result) => result;
        public static ParseResult Null(ParseResult _) => null;      
    }
}
