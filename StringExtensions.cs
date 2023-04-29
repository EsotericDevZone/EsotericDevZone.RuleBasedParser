using EsotericDevZone.Core.Collections;
using EsotericDevZone.RuleBasedParser.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static EsotericDevZone.RuleBasedParser.Helpers.CommentsStyleHelper;
using static EsotericDevZone.RuleBasedParser.Helpers.TokenSplitHelper;

namespace EsotericDevZone.RuleBasedParser
{
    internal static class StringExtensions
    {
        private static readonly Dictionary<char, string> CharsToEscape = new Dictionary<char, string>
        {
            { '\"', "\\\""},
            { '\\', @"\\"},
            { '\0', @"\0"},
            { '\a', @"\a"},
            { '\b', @"\b"},
            { '\f', @"\f"},
            { '\n', @"\n"},
            { '\r', @"\r"},
            { '\t', @"\t"},
            { '\v', @"\v"},
        };

        /// <summary>
        /// Converts a string value to its literal constant string form (e.g "my    string" => "\"my\tstring\"")
        /// </summary>                                
        public static string ToLiteral(this string input)
        {            
            StringBuilder literal = new StringBuilder(input.Length + 2);
            literal.Append("\"");
            foreach (var c in input)
            {
                if (CharsToEscape.ContainsKey(c))
                    literal.Append(CharsToEscape[c]);
                else if (c >= 0x20 && c <= 0x7e)
                    literal.Append(c);
                else 
                    literal.Append($@"\u{(int)c:x4}");
            }
            literal.Append("\"");
            return literal.ToString();
        }

        public static string FromLiteral(this string input)
        {
            var result = Regex.Replace(input, @"\\[\\""0abfnrtv]", m =>
            {
                switch (m.Value)
                {
                    case @"\\": return "\\";
                    case @"""": return "\"";
                    case @"\0": return "\0";
                    case @"\a": return "\a";
                    case @"\b": return "\b";
                    case @"\f": return "\f";
                    case @"\n": return "\n";
                    case @"\r": return "\r";
                    case @"\t": return "\t";
                    case @"\v": return "\v";
                    default: return m.Value;
                }
            });
            return Regex.Replace(result, @"\\u[A-Fa-f0-9]{4,6}", m =>
            {
                int code = int.Parse(m.Value, System.Globalization.NumberStyles.HexNumber);
                return char.ConvertFromUtf32(code).ToString();
            });                
        }

        /// <summary>
        /// Replaces multiple adjacent whitespaces with a single space (' ') character.
        /// E.g. "my  unevenly\t spaced string".RemoveRedundantWhitespace() --> "my unevenly spaced string"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>       
        public static IEnumerable<Match> RemoveRedundantWhitespace(this string input)
        {
            return new Regex(@"[^\s]+")
                .Matches(input)
                .Cast<Match>();                
        }

        public static IEnumerable<Token> FindComments(this string input, CommentStyle commentsStyle)
        {            
            var blocks = commentsStyle.BlockComments.Length==0 ? Lists.Empty<Match>() : Regex
                .Matches(input, GetBlockCommentsRegex(commentsStyle.BlockComments), RegexOptions.Singleline).Cast<Match>();
            var inlines = commentsStyle.InlineComments.Length == 0 ? Lists.Empty<Match>() : Regex
                .Matches(input, GetInlineCommentsRegex(commentsStyle.InlineComments), RegexOptions.Multiline).Cast<Match>();
            return blocks.Concat(inlines)
                .Select(m => new Token(m.Value, m.Index));
        }

        public static IEnumerable<Token> FindUnsplittableStrings(this string input, TokensSplitOptions options)
            => Regex.Matches(input, GetSplitBreakingRegexStringsOnly(options.SplitBreakingRules)).Cast<Match>()
                .Select(m=>new Token(m.Value, m.Index));


        // remove comments but take split blockers into account
        public static List<Token> RemoveComments(this string input, CommentStyle commentStyle, TokensSplitOptions tokensSplitOptions)
        {
            var _comments = input.FindComments(commentStyle);            
            var _strings = input.FindUnsplittableStrings(tokensSplitOptions);

            // remove strings completely included in comments
            _strings = _strings.Where(s => _comments
                .Any(c => c.IncludesIndex(s.Index) && c.IncludesIndex(s.Index + s.Length - 1)));            

            var _commentsCpy = _comments.ToArray();
            // remove comments included in other comments
            _comments = _comments.Where(inner => !_commentsCpy
                .Any(outer => outer.Index < inner.Index && inner.Index + inner.Length < outer.Index + outer.Length));

            // remove comments that start inside strings to obtain true comments
            var comments = _comments.Where(c => !_strings.Any(str => str.IncludesIndex(c.Index)))                
                .OrderBy(c => c.Index)
                .ToArray();

            comments = Lists.Of(new Token("", 0)).Concat(comments)
                .Concat(Lists.Of(new Token("", input.Length))).ToArray();



            /*Console.WriteLine("----------------------------------------");
            _strings.ToList().ForEach(c => Console.WriteLine($"{c.Index}:{c.Value}"));
            Console.WriteLine("----------------------------------------");
            comments.ToList().ForEach(c => Console.WriteLine($"{c.Index}:{c.Value}"));
            Console.WriteLine("----------------------------------------");*/

            List<Token> tokens = new List<Token>();

            // get values between comments            
            for (int i = 0; i < comments.Length - 1; i++) 
            {
                var thisComEnd = comments[i].Index + comments[i].Length;
                var nextComStart = comments[i + 1].Index;
                if (thisComEnd >= nextComStart) 
                    continue;

                var item = input.Substring(thisComEnd, nextComStart - thisComEnd);
                tokens.Add(new Token(item, thisComEnd));
            }

            /*Console.WriteLine("After removing comments:");

            tokens.ForEach(t => Console.WriteLine(t.Value));

            Console.WriteLine("----------------------");*/

            return tokens;
        }        
       
        private static IEnumerable<Token> ToTokens(this string[] splittedValues, int tokenStartIndex)
        {
            int offset = 0;
            foreach(var value in splittedValues)
            {
                yield return new Token(value, tokenStartIndex + offset);
                offset += value.Length;
            }
            yield break;
        }

        public static List<Token> SplitToTokens(this string input, TokensSplitOptions options, CommentStyle commentsStyle)
        {
            var splitBreakingRegex = GetSplitBreakingRegex(options.SplitBreakingRules);           
            //Console.WriteLine(splitBreakingRegex);
            //Console.WriteLine(options.Atoms.GetAtomsSplitRegex());            

            return input.RemoveComments(commentsStyle, options)
                // isolate "strings" from the rest
                .Select(token =>
                    Regex.Matches(token.Value, splitBreakingRegex).Cast<Match>()
                        .Select(m => new Token(m.Value, token.Index + m.Index))
                ).Flatten()
                // remove whitespace from non-strings
                .Select(token =>
                {
                    return token.Value.IsSplitBreakingString(options.SplitBreakingRules)
                        ? Lists.Of(token)
                        : token.Value.RemoveRedundantWhitespace()
                            .Select(m => new Token(m.Value, token.Index + m.Index));
                }).Flatten()
                // split by atoms
                .Select(token =>
                {
                    if (options.Atoms.GetAtomsSplitRegex() == "")
                        return Lists.Of(token);
                    return token.Value.IsSplitBreakingString(options.SplitBreakingRules)
                        ? Lists.Of(token)
                        : token.Value.RemoveRedundantWhitespace()
                            .Select(m => new Token(m.Value, m.Index + token.Index))
                            .Select(tk => Regex.Split(tk.Value, options.Atoms.GetAtomsSplitRegex())

                                .ToTokens(tk.Index)).Flatten();
                }).Flatten()
                // finish
                .Where(token => !string.IsNullOrWhiteSpace(token.Value))
                .ToList()
                .FixTokensPositions(input);                
        }

        private static List<Token> FixTokensPositions(this List<Token> tokens, string input)
        {
            if (tokens.Count() == 0) return Lists.Empty<Token>();

            var lines = input.Split('\n');
            int lineIndex = 0;
            int lineStartPos = 0;

            foreach(var tk in tokens)
            {
                while (tk.Index >= lineStartPos + lines[lineIndex].Length)
                {
                    lineStartPos += lines[lineIndex].Length + 1;
                    lineIndex++;
                }

                if(lineStartPos<=tk.Index && tk.Index + tk.Length <= lineStartPos + lines[lineIndex].Length)
                {
                    tk.Line = lineIndex + 1;
                    tk.Column = tk.Index - lineStartPos + 1;
                }
            }
            return tokens;
        }
    }
}
