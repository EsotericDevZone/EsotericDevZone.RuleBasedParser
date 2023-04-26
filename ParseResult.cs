namespace EsotericDevZone.RuleBasedParser
{
    public class ParseResult
    {
        public Token GeneratorToken { get; set; }
        public object Value { get; set; }

        public ParseResult(Token generatorToken, object value)
        {
            GeneratorToken = generatorToken;
            Value = value;
        }

        public override string ToString() => $"ParseResult(Token={GeneratorToken}, Value={Value})";
    }
}
