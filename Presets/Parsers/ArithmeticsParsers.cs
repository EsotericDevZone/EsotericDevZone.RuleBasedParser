using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser.Presets.Parsers
{
    public static class ArithmeticsParsers
    {
        public static ArithmeticsParser<int> Integers => new ArithmeticsParser<int>(AtomBuilders.Integer);
    }
}
