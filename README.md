# EsotericDevZone.RuleBasedParser

This library provides a simple token-oriented text parser featuring a set of user defined rules. 

## Parse rules

A parse rule is a structure described by a rule key (the rule's name which starts with an `@` character), a pattern and an object build method.

The rule key represents the type of the object which results after parsing the input. For example:
```
Rule key = "@ADDITION"
Pattern = "@TERM + @TERM" # so expressions of type 1+2, 15+67 etc are considered "@ADDITION"s
Builder = (term1, term2) -> term1+term2
```

The pattern tells the parser how to identify the input tokens. A pattern is a series of individual matchers separated by a single space `" "`.

The types of matchers are:

- **Atoms**

    An atom corresponds to a single token from the input interpreted in some specific way. Atom types are user defined. They are recommended to be named with capital letters for better distinction.
    
    For example, `NUMBER` atom matcher identifier any numeric values such as `7`, `13`, `12.3`, while `STRING` may match values like `"this string"`.
- **Rule keys**

    A rule key matcher tells the parser that it should look for an entity which corresponds to that rule key.

    Fore example, in the case of the pattern `TERM = @ADDITION`, the parser expects values like `1+4` at the right of the equality sign.

- **Repeatable Tail (`??`)** 

    Anything following the repeatable tail matcher (`??`) is optional and may match as many times as it can. 

    For example, the pattern `@TERM ?? + @TERM` mathes sequences of summed number of any length, like `1+7`, `5`, `2+3+20` etc.

    *The repeatable tail must appear exactly once in a rule pattern!*

- **Literals**

    A literal match defines a verbatim token which must appear in the input. 
    
    The `+` sign in the pattern `@TERM + @TERM` is an example of literal matcher.
    Similarly, the `function` keyword in pattern `function NAME ( )`, as well as the parenthesis `(`, `)` are also literals.

- **Wildcards**

    A wildcard is a list of possible literals separated by the `|` delimiter. The parser considers it the input matches a wildcard if the parsed token is
    one of the items in the literals list. 

    For example, to match both addition and subtractions, one can do `@TERM ?? +|- @TERM`. The parser matches expressions like `1+2`, `5-3`, `1+2-5`.

    **No spaces between the literals and delimiter!** Pattern `+ |-` is completely different from `+|-` and is a potential error source. The pattern `+ |-` describes a "rule" that matches the `+` sign followed by a wildcard consisting of a ghost token (the "one" before the delimiter `|`) or the `-` sign.

## Tokens

A token is an indivisible substring of the input. It can be imagined as a single word in a paragraph. By default, tokens are isolated from each other by any kind of whitespace.
In some context, that would be inconventient as the input `text that "contains strings"` is separated in 4 tokens (`text`, `that`, `"contains`, `strings"`) while the user might expect 3. Moreover, inputs like `1+2+3` would generate a single token while the human mind might usually identify 5 of them. There is a solution to overcome this problem.

There are `TokensSplitOptions` to tell in which situations token must not be splitted (therefore preserving spaces between them), or must be splitted, even if there is no explicit whitespace between them.

The `TokensSplitOptions` contains two sets of rules:

- **Split Breaking Rules**

    A split breaking rule consist of two characters meaning that anything that is found between them represents a single token. For example, the split breaking rule (`""`) identifies '"this token"', `"and this \" escaped one \""`, `"and even 'this'"`, but `'not`, ..., `this_one'`. Similarly, the rule (`{}`) isolated `{anything between brackets}`. When the two characters are identical, one can be skipped, such that the rule (`""`) is equivalent to (`"`).

- **Atoms**

    An atom (_in tokens terms_) is a regex that describes a single atom and isolates it from the adjacent context. For example, the atom rule (`\+`) isolates every _plus_ sign from the input `1+2+3`, leading to 5 tokens '1', `+`, `2`, `+`, `3`.

    **The atom rules are Regular Expressions, please be aware of the regex escaping rules when needed (A rule that isolates the `=` sign token must be written as `\=`)!**

    Some more complex rules can be defined, like `\d\.\d`, which matches any of these numbers (`1.2`, `3.5` etc.) *even when the atom rule `\.` has been defined*.

## Comments

In case portions of the input are wanted to be ignored, the user can define a `CommentStyle`, which is a way to identify and remove inline and block comments from the original input.

- **Inline comment rules** are described by a single string marking the start of the comment, like the slashes (`//`) in C/C++. An inline comment starts from the specified marker and keeps going until the end of the line.
- **Block comment rules** are described by two string markers, one for the begining (e.g. `/*`) and one for the end (e.g. `*/`) of the comment.

CommentStyles are optional.

## Creating a Parser (Demo)

Let's see how to create a parser that evaluates integer aritmetic expressions.

First, we need to define our tokens. A token is a continuous sequence of digits, or an operator (let's say the four basic operations `+ - * /`, or a parenthesis `()`).
We don't have split breaking rules, as no strings are involved.

Therefore, we define our token split options:
```C#
using EsotericDevZone.Core.Collections;
//...
var tokensSplitOptions = new TokensSplitOptions(
    Lists.Empty<string>(),  // no split breaking rules
    Lists.Of(@"\+", @"\-", @"\*", @"\/", @"\(", @"\)") // atom rules
);  
```

Let's create our parser:

```C# 

var parser = new Parser();
parser.TokensSplitOptions = tokensSplitOptions;
parser.CommentStyle = CommentStyles.NoCommentsStyle; // No comment :))
```

We need to tell the parser how to evaluate numbers. For that, we need to define an parse atom:

```C#
parser.RegisterAtom("NUMBER", AtomBuilders.Integer);
```

Or, if you're feeling like you want to do it yourself:

```C#
parser.RegisterAtom("NUMBER", (token) 
    => int.TryParse(input, out int value) 
        ? AtomResult.Atom(value) 
        : AtomResult.Error("Input is not an integer")
);                
```

Now let's see how to create the parse rules. We need to think for a while and consider the following situations:

- `12` - single number
- `6+3` - simple addition
- `1+2+3+4` - multiple addition
- `3+4-2` - combined addition/subtraction
- `5*6` - multiplication
- `1+5*6-3+8/3` - combined operations
- `6+(4+5)*3` - parenthesis

From these examples, we can extract a couple of helpful ideas:

1. A single number is a valid expression
1. A series of additions/subtractions is a valid expression
1. A series of multiplications/divisions is a valid expression
1. In a combined operations input, multiplication and division comes before addition/subtraction
1. Anything between parenthesis must be solved first

These hints point to a following rules system:

```
key: @EXPR  pattern: @ADD
key: @ADD   pattern: @MUL ?? +|- @MUL
key: @MUL   pattern: @TERM ?? *|/ @TERM
key: @TERM  pattern: NUMBER
key: @TERM  pattern: ( @EXPR )
```

Let's see how we translate them into code:

```C#
// Make the @EXPR evaluate as the result of its contained pattern rule @ADD :
parser.ParseRules.RegisterRule("@EXPR", "@ADD", ParseResultBuilders.Self); 

// Write the @ADD rule and how to evaluate it
parser.ParseRules.RegisterRule("@ADD", "@MUL ?? +|- @MUL", ParseResultBuilders.LeftAssociate((a, b, sign) =>
    {
        // LeftAssociate(oper) takes a list of N parsed (@MUL) entities and N-1 tokens/operators
        // and sequencially calls result = oper(result, value(i), token(i-1))
        // consuming each of the tokens from left to right        
        if (sign.Value == "+")
            return new ParseResult(sign, (int)a.Value + (int)b.Value);
        else // if (sign.Value == "-")
            return new ParseResult(sign, (int)a.Value - (int)b.Value);
    }));

// Do the same for @MUL rule
parser.ParseRules.RegisterRule("@MUL", "@TERM ?? *|/ @TERM", ParseResultBuilders.LeftAssociate((a, b, sign) =>
    {
        if (sign.Value == "*")
            return new ParseResult(sign, (int)a.Value * (int)b.Value);
        else // if (sign.Value == "/")
            return new ParseResult(sign, (int)a.Value / (int)b.Value);
    }));

// Define @TERM rulekey. Multiple rule definitions with the same key are allowed.
// If one rule fails to parse, the others are tried in the order they have been declared
parser.ParseRules.RegisterRule("@TERM", "NUMBER", ParseResultBuilders.Self);
// Self builder works even when there are also literals around the target pattern :
parser.ParseRules.RegisterRule("@TERM", "( @EXPR )", ParseResultBuilders.Self); 
```

Finally, we have to tell the parser what the input is as a whole (what it should look for when it parses the input). 
We do that by specifying something that's called the `RootRuleKey` - the "global" parse rule pattern:

```C#
parser.RootRuleKey = "@EXPR"; // we are generally looking for "expressions"
```

Now we are ready to go:

```C#
int result1 = parser.Parse("(1+2)*4"); // output: 12
int result2 = parser.Parse("3 - 2-1"); // output: 0
```

Or, make it interactive:

```C#
 while (true)
{
    try
    {
        Console.Write(">> ");
        string input = Console.ReadLine();
        Console.WriteLine(parser.Parse(input));
    }
    catch(Exception e)
    {
        Console.WriteLine(e.Message);
    }
}

/*                 Output
---------------------------------------------
    >> 1+2
    3
    >> 165*887
    146355
    >> 5+6*(3-5*(7-3))+4
    -93
    >> 1+
    1:2 Parse error: Insuficient tokens
    >> 45 6
    1:4 Parse error: Insuficient tokens
    >> 10
    10
---------------------------------------------*/
```