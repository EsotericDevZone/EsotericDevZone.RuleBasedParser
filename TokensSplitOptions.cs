using EsotericDevZone.Core.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EsotericDevZone.RuleBasedParser
{
    public class TokensSplitOptions
    {
        /// <summary>
        /// List of rules that mark a literal sequence which must not be splitted (e.g. quotes, double quotes, brackets etc).
        /// </summary>
        /// <remarks>
        /// If a pair of two characters is specified as a split breaking rule, it means that the literal sequence 
        /// starts with the first split breaking character and ends with the second one.
        /// </remarks>
        /// <example>
        /// - "'"  is a split breaking rule for 'this string'
        /// - "{}" is a split breaking rule for {this string}        
        /// </example>
        public string[] SplitBreakingRules { get; }

        /// <summary>
        /// List of atom strings. Two atoms next to each other must be separated after split even if there
        /// is no whitespace between them (e.g. "+-" --> "+", "-")
        /// </summary>
        public string[] Atoms { get; }

        public TokensSplitOptions(IEnumerable<string> splitBreakingRules, IEnumerable<string> atoms)
        {
            SplitBreakingRules = splitBreakingRules.ToArray();
            Atoms = atoms.ToArray();
        }

        public TokensSplitOptions() : this(Collections.EmptyList<string>(), Collections.EmptyList<string>()) { }
    }
}
