using EsotericDevZone.Core.Extensions;

namespace EsotericDevZone.RuleBasedParser
{ 
    public class Token
    {
        public string Value { get; }
        public int Index { get; }

        public Token(string value, int index)
        {
            Value = value;
            Index = index;
        }

        public int Length => Value.Length;        

        public bool IncludesIndex(int index) => index.IsBetween(Index, Index + Length - 1);

        public override string ToString() => $"Token \"{Value}\" at index {Index} = {Line}:{Column}";

        public int Line { get; internal set; }
        public int Column { get; internal set; }
    }
}
