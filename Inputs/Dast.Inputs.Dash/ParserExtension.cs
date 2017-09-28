using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace Dast.Inputs.Dash
{
    static public class ParserExtension
    {
        static public TContext TwoStageParsing<TParser, TContext>(this TParser parser, Func<TParser, TContext> func)
            where TParser : Parser
        {
            parser.Interpreter.PredictionMode = PredictionMode.Sll;
            parser.ErrorHandler = new BailErrorStrategy();

            TContext context;
            try
            {
                context = func(parser);
            }
            catch (ParseCanceledException)
            {
                parser.Reset();

                parser.Interpreter.PredictionMode = PredictionMode.Ll;
                parser.ErrorHandler = new DefaultErrorStrategy();
                context = func(parser);
            }

            return context;
        }
    }
}