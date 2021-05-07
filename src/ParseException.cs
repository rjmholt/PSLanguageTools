using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace PSLanguageTools
{
    public class ParseException : Exception
    {
        public ParseException(IReadOnlyList<ParseError> parseErrors)
            : base("A parse error was encountered while parsing the input script")
        {
            ParseErrors = parseErrors;
        }

        public IReadOnlyList<ParseError> ParseErrors { get; }
    }
}

