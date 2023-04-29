using EsotericDevZone.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericDevZone.RuleBasedParser.Presets.Parsers
{
    public class ArithmeticsParser<T> : Parser
    {
        private Func<string, object> NumberBuilder;        

        public ArithmeticsParser(Func<string, object> numberBuilder) : base()
        {
            this.TokensSplitOptions = new TokensSplitOptions(
                Lists.Empty<string>(),
                Lists.Of(@"\+", @"\-", @"\*", @"\/", @"\(", @"\)")
                );
            base.CommentStyle = CommentStyles.NoCommentsStyle;
            NumberBuilder = numberBuilder;
            Initialize();
        }

        private void Initialize()
        {
            RegisterAtom("NUMBER", NumberBuilder);

            ParseRules.RegisterRule("@E", "@E1 ?? +|- @E1", ParseResultBuilders
                .LeftAssociate((a, b, tk) =>
                {
                    if (tk.Value == "+")
                        return new ParseResult(tk, (T)((dynamic)a.Value + (dynamic)b.Value));
                    else
                        return new ParseResult(tk, (T)((dynamic)a.Value - (dynamic)b.Value));
                }));

            ParseRules.RegisterRule("@E1", "@T ?? *|/|% @T", ParseResultBuilders
                .LeftAssociate((a, b, tk) =>
                {
                    if (tk.Value == "*")
                        return new ParseResult(tk, (T)((dynamic)a.Value * (dynamic)b.Value));
                    else if (tk.Value == "/")
                        return new ParseResult(tk, (T)((dynamic)a.Value / (dynamic)b.Value));
                    else
                        return new ParseResult(tk, (T)((dynamic)a.Value % (dynamic)b.Value));
                }));
                        
            ParseRules.RegisterRule("@T", "NUMBER", ParseResultBuilders.Self);
            ParseRules.RegisterRule("@T", "SYMBOL", ParseResultBuilders.Self);
            ParseRules.RegisterRule("@T", "( @E )", ParseResultBuilders.Self);

            RootRuleKey = "@E";
        }        

    }
}
