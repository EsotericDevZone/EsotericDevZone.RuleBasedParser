using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser
{
    public class AtomResult
    {
        public object Value { get; }
        public string ErrorMessage { get; }

        public bool Failed => ErrorMessage != null;

        private AtomResult(object value, string errorMessage)
        {
            Value = value;
            ErrorMessage = errorMessage;
        }        

        public static AtomResult Atom(object value)
        {            
            return new AtomResult(value, null);
        }
        public static AtomResult Error(string message)
        {            
            return new AtomResult(null, message);
        }

        public override string ToString() => Failed ? $"AtomResult Error={ErrorMessage}"
            : $"AtomResult Value={Value}";

    }
}
