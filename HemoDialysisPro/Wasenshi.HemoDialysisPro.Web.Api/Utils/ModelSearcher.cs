using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils
{
    public abstract class ModelSearcher
    {
        // special operator for advanced FE search/filter
        protected static readonly string[] operators = { " and ", " or ", "&", "&&", "|", "||", "(", ")" };
        protected static readonly char[] opChars = operators.Select(x => x[0]).Distinct().ToArray();

        public static IEnumerable<string> Tokenize(string input, bool breakWhitespace, params string[] operators)
        {
            char[] opChars = operators.SelectMany(x => x.ToCharArray()).Distinct().ToArray();
            if (opChars.Length == 0)
            {
                opChars = ModelSearcher.opChars;
                operators = ModelSearcher.operators;
            }
            bool hasSpaceBasedOp = opChars.Contains(' ');

            var buffer = new StringBuilder();
            foreach (char c in input)
            {
                if (IsOperatorChar(c, opChars))
                {
                    if (buffer.Length > 0)
                    {
                        // we have back-buffer; could be &, but could be &&
                        // need to check if there is a combined operator candidate
                        if (!CanCombine(buffer, c, operators) &&
                            (!hasSpaceBasedOp ||
                            hasSpaceBasedOp && (breakWhitespace || buffer[buffer.Length - 1] == ' ' || c != ' ')))
                        {
                            var s = Flush(buffer).Trim();
                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                yield return s;
                            }
                        }
                    }
                    buffer.Append(c);
                    continue;
                }

                if (char.IsWhiteSpace(c) && breakWhitespace)
                {
                    if (buffer.Length > 0)
                    {
                        yield return Flush(buffer);
                    }

                    continue; // just skip whitespace
                }

                // so here, the new character is *not* an operator; if we have
                // a back-buffer that *is* operators, yield that
                if (buffer.Length > 0 && HasOperator(buffer, hasSpaceBasedOp, opChars))
                {
                    var s = Flush(buffer).Trim();
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        yield return s;
                    }
                }

                // append
                buffer.Append(c);
            }
            // out of chars... anything left?
            if (buffer.Length != 0)
            {
                var s = Flush(buffer).Trim();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    yield return s;
                }
            }
        }

        private static string Flush(StringBuilder buffer)
        {
            string s = buffer.ToString();
            buffer.Clear();
            return s;
        }

        private static bool IsOperatorChar(char newChar, char[] opChars)
        {
            return Array.IndexOf(opChars, newChar) > -1;
        }
        private static bool HasOperator(StringBuilder buffer, bool hasSpaceBasedOp, char[] opChars)
        {
            var checkValue = hasSpaceBasedOp ? buffer[0] : buffer.ToString().TrimStart().FirstOrDefault();
            if (checkValue == default(char))
            {
                return false;
            }
            return Array.IndexOf(opChars, checkValue) > -1;
        }
        private static bool CanCombine(StringBuilder buffer, char c, string[] operators)
        {
            var checkValue = buffer.ToString().TrimStart();
            foreach (var op in operators)
            {
                var checkOp = op.TrimStart();

                if (checkOp.Length <= checkValue.Length) continue;
                // check starts with same plus this one
                bool startsWith = true;
                for (int i = 0; i < checkValue.Length; i++)
                {
                    if (checkOp[i] != checkValue[i])
                    {
                        startsWith = false;
                        break;
                    }
                }
                if (startsWith && checkOp[checkValue.Length] == c) return true;
            }
            return false;
        }
    }

    public abstract class ModelSearcher<T> : ModelSearcher
    {
        public Expression<Func<T, bool>> GetWhereCondition(string whereString)
        {
            var where = whereString.ToLower();

            var stack = new Stack<Expression<Func<T, bool>>>();
            var result = ParseRecursive(Tokenize(where, false), stack);

            if (stack.Count == 0)
            {
                return ParseDefault(where);
            }

            if (stack.Count != 1) // expression error case
            {
                return null;
            }

            return result;
        }

        protected Expression<Func<T, bool>> ParseRecursive(IEnumerable<string> conditionTokens,
            Stack<Expression<Func<T, bool>>> mainStack, Stack<Expression<Func<T, bool>>> pre = null)
        {
            if (mainStack == null)
            {
                mainStack = new Stack<Expression<Func<T, bool>>>();
            }
            // handle parenthesis and recursive
            var tokenStack = new Stack<string>();
            var tmpPre = new Stack<Expression<Func<T, bool>>>();
            foreach (var token in conditionTokens)
            {
                if (token == ")")
                {
                    var tmpStack = new Stack<Expression<Func<T, bool>>>();
                    Stack<string> sub = new Stack<string>();
                    while (tokenStack.Count > 0 && tokenStack.Peek() != "(")
                    {
                        sub.Push(tokenStack.Pop());
                    }
                    tokenStack.Pop();

                    var prev = sub.Count % 2 == 0 ? tmpPre : null; // important! determine whether this is recursive from prev level or not
                    var subResult = ParseRecursive(sub, tmpStack, prev);
                    if (subResult == null) // cut-circuit
                    {
                        return null;
                    }
                    // clear stack to fix the order
                    sub.Clear();
                    while (tokenStack.Count > 0 && tokenStack.Peek() != "(")
                    {
                        sub.Push(tokenStack.Pop());
                    }
                    // push all remaining and parse right away to preserve ordering
                    subResult = ParseRecursive(sub, tmpStack, tmpPre);
                    if (subResult == null) // cut-circuit
                    {
                        return null;
                    }

                    tmpPre.Push(tmpStack.Pop());
                }
                else
                {
                    tokenStack.Push(token);
                }
            }

            int count = tokenStack.Count;

            var opStack = new Stack<char>();
            while (tokenStack.TryPop(out string token))
            {
                if (token == "and" || token == "&" || token == "&&")
                {
                    opStack.Push('&');
                    continue;
                }
                if (token == "or" || token == "|" || token == "||")
                {
                    opStack.Push('|');
                    continue;
                }

                mainStack.Push(ParseConditionBlock(token)); // Each block parsing is in this line
            }
            while (tmpPre?.Count > 0)
            {
                mainStack.Push(tmpPre.Pop());
            }
            while (pre?.Count > 0) // previous calculation
            {
                mainStack.Push(pre.Pop());
            }

            // cut for default case to handle
            if (count == 1 && mainStack.Count > 0 && mainStack.Peek() == null)
            {
                return mainStack.Pop();
            }

            if (opStack.Count >= mainStack.Count) // expression error case
            {
                return null;
            }

            while (opStack.TryPop(out char op))
            {
                var first = mainStack.Pop();
                if (first == null)
                {
                    return null;
                }
                var second = mainStack.Pop();
                if (second == null)
                {
                    return null;
                }
                if (op == '&')
                {
                    mainStack.Push(first.AndAlso(second));
                }
                else if (op == '|')
                {
                    mainStack.Push(first.OrElse(second));
                }
                else
                {
                    throw new InvalidOperationException("wrong operation!: " + op);
                }
            }

            return mainStack.Peek();
        }

        protected abstract Expression<Func<T, bool>> ParseDefault(string whereString);
        protected abstract Expression<Func<T, bool>> ParseConditionBlock(string whereString);

        protected sealed class ModelReplacer<TBaseModel> : ExpressionVisitor
        {
            private readonly Expression<Func<T, TBaseModel>> stockGetter;

            public ModelReplacer(Expression<Func<T, TBaseModel>> getStock)
            {
                stockGetter = getStock;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return stockGetter.Parameters[0];
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.NodeType == ExpressionType.MemberAccess && node.Expression.Type == typeof(TBaseModel))
                {
                    return Expression.MakeMemberAccess(stockGetter.Body, node.Member);
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitLambda<TSource>(Expression<TSource> node)
            {
                return Expression.Lambda(Visit(node.Body), VisitAndConvert(node.Parameters, "VisitLambda"));
            }
        }
    }
}
