using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Language;

namespace PSLanguageTools.PrettyPrinting
{
    /// <summary>
    /// Prints a PowerShell AST based on its structure rather than text captured in extents.
    /// </summary>
    public abstract class PrettyPrinter
    {
        private readonly PrettyPrintingVisitor _visitor;

        /// <summary>
        /// Create a new pretty printer for use.
        /// </summary>
        protected PrettyPrinter()
        {
            _visitor = new PrettyPrintingVisitor();
        }

        protected abstract TextWriter CreateTextWriter();

        protected virtual void DoPostPrintAction()
        {
        }

        /// <summary>
        /// Pretty print a PowerShell script provided as an inline string.
        /// </summary>
        /// <param name="input">The inline PowerShell script to parse and pretty print.</param>
        /// <returns>A pretty-printed version of the given PowerShell script.</returns>
        protected void DoPrettyPrintInput(string input)
        {
            Ast ast = Parser.ParseInput(input, out Token[] tokens, out ParseError[] errors);

            if (errors != null && errors.Length > 0)
            {
                throw new ParseException(errors);
            }

            DoPrettyPrintAst(ast, tokens);
        }

        /// <summary>
        /// Pretty print the contents of a PowerShell file.
        /// </summary>
        /// <param name="filePath">The path of the PowerShell file to pretty print.</param>
        /// <returns>The pretty-printed file contents.</returns>
        protected void DoPrettyPrintFile(string filePath)
        {
            Ast ast = Parser.ParseFile(filePath, out Token[] tokens, out ParseError[] errors);

            if (errors != null && errors.Length > 0)
            {
                throw new ParseException(errors);
            }

            DoPrettyPrintAst(ast, tokens);
        }

        /// <summary>
        /// Pretty print a given PowerShell AST.
        /// </summary>
        /// <param name="ast">The PowerShell AST to print.</param>
        /// <param name="tokens">The token array generated when the AST was parsed. May be null.</param>
        /// <returns>The pretty-printed PowerShell AST in string form.</returns>
        protected void DoPrettyPrintAst(Ast ast, IReadOnlyList<Token> tokens)
        {
            using (TextWriter textWriter = CreateTextWriter())
            {
                _visitor.Run(textWriter, ast, tokens);
                DoPostPrintAction();
            }
        }
    }
}

