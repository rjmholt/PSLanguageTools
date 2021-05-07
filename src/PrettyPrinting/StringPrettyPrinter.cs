using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Language;

namespace PSLanguageTools.PrettyPrinting
{
    public class StringPrettyPrinter : PrettyPrinter
    {
        private string _result;

        private StringWriter _sw;

        public string PrettyPrintInput(string input)
        {
            DoPrettyPrintInput(input);
            return _result;
        }

        public string PrettyPrintFile(string filePath)
        {
            DoPrettyPrintFile(filePath);
            return _result;
        }

        public string PrettyPrintAst(Ast ast, IReadOnlyList<Token> tokens)
        {
            DoPrettyPrintAst(ast, tokens);
            return _result;
        }

        protected override TextWriter CreateTextWriter()
        {
            _sw = new StringWriter();
            return _sw;
        }

        protected override void DoPostPrintAction()
        {
            _result = _sw.ToString();
        }
    }
}

