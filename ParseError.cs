using EsotericDevZone.RuleBasedParser.ParseRulePatterns;

namespace EsotericDevZone.RuleBasedParser
{
    internal class ParseError : IParseRulePatternItemMatch
    {
        public string Message { get; }
        public int Position { get; }

        public int TokensCount { get; } = 0;

        public double Similarity { get; set; } = 0;

        public ParseError(ParseError error, double similarity)
        {
            Message = error.Message;
            Position = error.Position;
            Similarity = similarity;
        }

        public ParseError(string message, int position)
        {
            Message = message;
            Position = position;
        }

        public override string ToString() => $"ParseError: \"{Message}\" at token index {Position} [S={Similarity}]";
    }
}
