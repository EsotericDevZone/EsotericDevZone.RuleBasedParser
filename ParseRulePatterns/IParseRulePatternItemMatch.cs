namespace EsotericDevZone.RuleBasedParser.ParseRulePatterns
{
    internal interface IParseRulePatternItemMatch 
    {
        int Position { get; }
        int TokensCount { get; }
        double Similarity { get; set; }
    }    
}
