using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser
{
    public class ParseException : Exception
    {
        public Token ReferencedToken { get; } = null;

        public ParseException() : base() { }
        public ParseException(string message) : base(message) { }
        public ParseException(Token token, string message) : base(message) { ReferencedToken = token; }
        public ParseException(Exception exception) : base(exception.Message, exception) { }
        public ParseException(ParseException exception) : this(exception.ReferencedToken, exception.Message) { }

        public override string Message 
        {
            get
            {
                if (ReferencedToken == null)
                    return $"Parse error: {base.Message}";
                else
                    return $"{ReferencedToken.Line}:{ReferencedToken.Column} Parse error: {base.Message}";
            }
        }

        public override string ToString() => Message;        
    }
}
