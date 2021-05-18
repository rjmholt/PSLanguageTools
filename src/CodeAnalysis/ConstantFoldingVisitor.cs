using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace PSLanguageTools.CodeAnalysis
{
    public class ConstantFoldingVisitor : AstVisitor2
    {
        private readonly Dictionary<string, object> _constantValues;

        public ConstantFoldingVisitor()
        {
            _constantValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyDictionary<string, object> GetConstantValues()
        {
            return new Dictionary<string, object>(_constantValues);
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            var valueVisitor = new SafeGetValueVisitor(_constantValues);
            try
            {
                assignmentStatementAst.Visit(valueVisitor);
            }
            catch (NonConstantValueException)
            {
                // This wasn't a constant value -- ignore it
            }
            return AstVisitAction.SkipChildren;
        }
    }
}