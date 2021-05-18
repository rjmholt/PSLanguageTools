using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;

namespace PSLanguageTools.CodeAnalysis
{
    public class SafeGetValueVisitor : ICustomAstVisitor2
    {
        private static readonly IReadOnlyDictionary<string, object> s_constantVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            { "true", true },
            { "false", false },
            { "null", null },
        };

        private readonly Dictionary<string, object> _knownValues;

        private readonly Dictionary<string, object> _temporaryVariables;

        public SafeGetValueVisitor() : this(knownValues: null)
        {
        }

        public SafeGetValueVisitor(Dictionary<string, object> knownValues)
        {
            _knownValues = knownValues;
            _temporaryVariables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            return GetEnumerable(arrayExpressionAst.SubExpression.Visit(this));
        }

        public object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            var result = new object[arrayLiteralAst.Elements.Count];
            for (int i = 0; i < arrayLiteralAst.Elements.Count; i++)
            {
                result[i] = arrayLiteralAst.Elements[i].Visit(this);
            }
            return result;
        }

        public object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            object rhs = assignmentStatementAst.Right.Visit(this);
            switch (assignmentStatementAst.Left)
            {
                case VariableExpressionAst variable:
                    _knownValues[variable.VariablePath.UserPath] = rhs;
                    break;

                default:
                    throw new NotImplementedException();
            }

            return rhs;
        }

        public object VisitAttribute(AttributeAst attributeAst)
        {
            throw new NotImplementedException();
        }

        public object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitBaseCtorInvokeMemberExpression(BaseCtorInvokeMemberExpressionAst baseCtorInvokeMemberExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            object left = binaryExpressionAst.Left.Visit(this);
            object right = binaryExpressionAst.Right.Visit(this);

            switch (binaryExpressionAst.Operator)
            {
                case TokenKind.Plus:
                    return Add(left, right);

                case TokenKind.Multiply:
                    return Multiply(left, right);

                case TokenKind.Minus:
                    return Subtract(left, right);

                case TokenKind.And:
                    return Convert<bool>(left) && Convert<bool>(right);

                case TokenKind.Or:
                    return Convert<bool>(left) || Convert<bool>(right);

                case TokenKind.Band:
                    return Convert<int>(left) & Convert<int>(right);
                
                case TokenKind.Bor:
                    return Convert<int>(left) | Convert<int>(right);

                case TokenKind.Ieq:
                    return Compare(left, right) == 0;

                case TokenKind.Ceq:
                    return Compare(left, right, caseSensitive: true) == 0;

                case TokenKind.Ine:
                    return Compare(left, right) != 0;

                case TokenKind.Cne:
                    return Compare(left, right, caseSensitive: true) != 0;

                case TokenKind.Ile:
                    return Compare(left, right) <= 0;

                case TokenKind.Cle:
                    return Compare(left, right, caseSensitive: true) <= 0;

                case TokenKind.Ilt:
                    return Compare(left, right) < 0;

                case TokenKind.Clt:
                    return Compare(left, right, caseSensitive: true) < 0;

                case TokenKind.Ige:
                    return Compare(left, right) >= 0;

                case TokenKind.Cge:
                    return Compare(left, right, caseSensitive: true) >= 0;

                case TokenKind.Igt:
                    return Compare(left, right) > 0;

                case TokenKind.Cgt:
                    return Compare(left, right, caseSensitive: true) > 0;

                default:
                    throw new NonConstantValueException();
            }
        }

        public object VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            throw new NotImplementedException();
        }

        public object VisitCommand(CommandAst commandAst)
        {
            throw new NonConstantValueException();
        }

        public object VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            return commandExpressionAst.Expression.Visit(this);
        }

        public object VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            throw new NotImplementedException();
        }

        public object VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return constantExpressionAst.Value;
        }

        public object VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            Type targetType = convertExpressionAst.Type.TypeName.GetReflectionType();

            if (targetType is null)
            {
                throw new NonConstantValueException();
            }

            return Convert(convertExpressionAst.Child.Visit(this), targetType);
        }

        public object VisitDataStatement(DataStatementAst dataStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitDynamicKeywordStatement(DynamicKeywordStatementAst dynamicKeywordAst)
        {
            throw new NotImplementedException();
        }

        public object VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            var sb = new StringBuilder();

            int expIdx = 0;
            // Start at 1, since the first character is the '"'
            int i = 1;
            int startOffset = expandableStringExpressionAst.Extent.StartOffset;
            ExpressionAst nextExpression = null;
            while (i < expandableStringExpressionAst.Extent.Text.Length
                && expIdx < expandableStringExpressionAst.NestedExpressions.Count)
            {
                nextExpression = expandableStringExpressionAst.NestedExpressions[expIdx];

                // Pave the substring between us and the next expandable expression
                int expStart = nextExpression.Extent.StartOffset - startOffset;
                if (i < expStart)
                {
                    sb.Append(expandableStringExpressionAst.Extent.Text.Substring(i, expStart - i));
                }

                // Now add the next expandable expression
                sb.Append(Flatten(nextExpression.Visit(this)));

                // Move past the expression
                i = nextExpression.Extent.EndOffset - startOffset;
                expIdx++;
            }

            // Put down any remaining part of the string between the last nested expression and the end
            int remainingLength = expandableStringExpressionAst.Extent.Text.Length - i - 1;
            if (remainingLength > 0)
            {
                sb.Append(expandableStringExpressionAst.Extent.Text.Substring(i, remainingLength));
            }

            return sb.ToString();
        }

        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            IEnumerable enumerable = GetEnumerable(forEachStatementAst.Condition.Visit(this));

            var acc = new List<object>();
            foreach (object value in enumerable)
            {
                _temporaryVariables[forEachStatementAst.Variable.VariablePath.UserPath] = value;

                object result = forEachStatementAst.Body.Visit(this);
                switch (result)
                {
                    case object[] array:
                        acc.AddRange(array);
                        break;

                    default:
                        acc.Add(result);
                        break;
                }
            }

            _temporaryVariables.Remove(forEachStatementAst.Variable.VariablePath.UserPath);

            return acc.ToArray();
        }

        public object VisitForStatement(ForStatementAst forStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitFunctionMember(FunctionMemberAst functionMemberAst)
        {
            throw new NotImplementedException();
        }

        public object VisitHashtable(HashtableAst hashtableAst)
        {
            var hashtable = new Hashtable();
            foreach (Tuple<ExpressionAst, StatementAst> entry in hashtableAst.KeyValuePairs)
            {
                object key = entry.Item1.Visit(this);
                object value = entry.Item2.Visit(this);
                hashtable.Add(key, value);
            }
            return hashtable;
        }

        public object VisitIfStatement(IfStatementAst ifStmtAst)
        {
            throw new NotImplementedException();
        }

        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            var array = Convert<object[]>(indexExpressionAst.Target.Visit(this));
            return array[Convert<int>(indexExpressionAst.Index.Visit(this))];
        }

        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            throw new NotImplementedException();
        }

        public object VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            throw new NotImplementedException();
        }

        public object VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            throw new NotImplementedException();
        }

        public object VisitParameter(ParameterAst parameterAst)
        {
            throw new NotImplementedException();
        }

        public object VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            return parenExpressionAst.Pipeline.Visit(this);
        }

        public object VisitPipeline(PipelineAst pipelineAst)
        {
            if (pipelineAst.PipelineElements.Count == 1)
            {
                return pipelineAst.PipelineElements[0].Visit(this);
            }

            throw new NotImplementedException();
        }

        public object VisitPropertyMember(PropertyMemberAst propertyMemberAst)
        {
            throw new NotImplementedException();
        }

        public object VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            throw new NotImplementedException();
        }

        public object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            if (!(statementBlockAst.Traps is null) && statementBlockAst.Traps.Count > 0)
            {
                throw new NonConstantValueException();
            }

            var list = new List<object>();
            foreach (StatementAst statement in statementBlockAst.Statements)
            {
                foreach (object result in GetEnumerable(statement.Visit(this)))
                {
                    list.Add(result);
                }
            }
            return list.ToArray();
        }

        public object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            switch (stringConstantExpressionAst.StringConstantType)
            {
                case StringConstantType.BareWord:
                    throw new NonConstantValueException();

                default:
                    return stringConstantExpressionAst.Value;
            }
        }

        public object VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            return subExpressionAst.SubExpression.Visit(this);
        }

        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitTrap(TrapStatementAst trapStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitTryStatement(TryStatementAst tryStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            throw new NotImplementedException();
        }

        public object VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            return typeExpressionAst.TypeName.GetReflectionType() ?? throw new NonConstantValueException();
        }

        public object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitUsingStatement(UsingStatementAst usingStatement)
        {
            throw new NotImplementedException();
        }

        public object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            if (s_constantVariables.TryGetValue(variableExpressionAst.VariablePath.UserPath, out object value))
            {
                return value;
            }

            if (!(_knownValues is null)
                && _knownValues.TryGetValue(variableExpressionAst.VariablePath.UserPath, out value))
            {
                return value;
            }

            if (_temporaryVariables.TryGetValue(variableExpressionAst.VariablePath.UserPath, out value))
            {
                return value;
            }

            throw new NonConstantValueException();
        }

        public object VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            throw new NotImplementedException();
        }

        private object Add(object left, object right)
        {
            switch (left)
            {
                case int lInt:
                    return lInt + Convert<int>(right);

                case long lLong:
                    return lLong + Convert<long>(right);

                case double lDouble:
                    return lDouble + Convert<double>(right);

                case float lFloat:
                    return lFloat + Convert<float>(right);

                case bool lBool:
                    return Convert<int>(lBool) + Convert<int>(right);

                case DateTime lDate:
                    return lDate.AddTicks(Convert<long>(right));

                case string lStr:
                    return lStr + Convert<string>(right);

                case IDictionary lDict:
                    var rDict = Convert<IDictionary>(right);
                    var result = new Hashtable(lDict);
                    foreach (DictionaryEntry entry in rDict)
                    {
                        result.Add(entry.Key, entry.Value);
                    }
                    return result;

                case IEnumerable lArray:
                    IEnumerable rArray = GetEnumerable(right);
                    var list = new List<object>();
                    foreach (object lObj in lArray)
                    {
                        list.Add(lObj);
                    }
                    foreach (object rObj in rArray)
                    {
                        list.Add(rArray);
                    }
                    return list.ToArray();

                default:
                    return Convert<string>(left) + Convert<string>(right);
            }
        }

        private object Multiply(object left, object right)
        {
            switch (left)
            {
                case int lInt:
                    return lInt * Convert<int>(right);

                case long lLong:
                    return lLong * Convert<long>(right);

                case double lDouble:
                    return lDouble * Convert<double>(right);

                case float lFloat:
                    return lFloat * Convert<float>(right);

                default:
                    throw new NonConstantValueException();
            }
        }

        private object Subtract(object left, object right)
        {
            switch (left)
            {
                case int lInt:
                    return lInt - Convert<int>(right);

                case long lLong:
                    return lLong - Convert<long>(right);

                case double lDouble:
                    return lDouble - Convert<double>(right);

                case float lFloat:
                    return lFloat - Convert<float>(right);

                case bool lBool:
                    return Convert<int>(lBool) - Convert<int>(right);

                default:
                    throw new NonConstantValueException();
            }
        }

        private int Compare(object left, object right, bool caseSensitive = false)
        {
            switch (left)
            {
                case int lInt:
                    return Comparer<int>.Default.Compare(lInt, Convert<int>(right));

                case long lLong:
                    return Comparer<long>.Default.Compare(lLong, Convert<long>(right));

                case double lDouble:
                    return Comparer<double>.Default.Compare(lDouble, Convert<double>(right));

                case float lFloat:
                    return Comparer<float>.Default.Compare(lFloat, Convert<float>(right));

                case bool lBool:
                    return Comparer<bool>.Default.Compare(lBool, Convert<bool>(right));

                case string lStr:
                    return caseSensitive
                        ? StringComparer.Ordinal.Compare(lStr, Convert<string>(right))
                        : StringComparer.OrdinalIgnoreCase.Compare(lStr, Convert<string>(right));

                case DateTime lDate:
                    return Comparer<DateTime>.Default.Compare(lDate, Convert<DateTime>(right));

                default:
                    throw new NonConstantValueException();
            }
        }

        private IEnumerable GetEnumerable(object value)
        {
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(value);

            return enumerable ?? new object[] { value };
        }

        private T Convert<T>(object value) => (T)Convert(value, typeof(T));

        private object Convert(object value, Type targetType) => LanguagePrimitives.ConvertTo(Flatten(value), targetType);

        private object Flatten(object value)
        {
            if (value is object[] array
                && array.Length == 1)
            {
                return array[0];
            }

            return value;
        }
    }
}