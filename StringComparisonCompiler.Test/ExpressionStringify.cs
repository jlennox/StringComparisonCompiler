using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace StringComparisonCompiler.Test
{
    public class ExpressionStringify
    {
        // This is REALLY HACKY. Unfortunately, the built in stringification fails and produces only "{ ... }"
        // The goal of this is to generate code that is both easy to read and able to be copy and pasted into a test
        // for further debugging.
        public static string Stringify(Expression exp, int indent = 0)
        {
            var sb = new StringBuilder();

            string Substring(Expression exp)
            {
                return Stringify(exp, indent);
            }

            void Append(string s)
            {
                sb.Append(' ', indent * 4);
                sb.AppendLine(s);
            }

            if (exp is BlockExpression block)
            {
                //Append("{");
                //indent++;

                foreach (var x in block.Expressions)
                {
                    Append(Stringify(x, indent));
                }

                //indent--;
                //Append("}");
            }
            else if (exp is GotoExpression gt)
            {
                Append("return " + Substring(gt.Value) + ";");
            }
            else if (exp is ConditionalExpression ce2 &&
                ce2.IfTrue is not GotoExpression &&
                ce2.IfFalse is not null)
            {
                Append($"({Substring(ce2.Test)})");
                indent++;
                Append($"? ({Substring(ce2.IfTrue)})");
                Append($": ({Substring(ce2.IfFalse)})");
                indent--;
            }
            else if (exp is ConditionalExpression ce)
            {
                Append($"if ({Substring(ce.Test)}) {{");
                indent++;
                Append(Stringify(ce.IfTrue, indent));

                if (!(ce.IfFalse is DefaultExpression))
                {
                    indent--;
                    Append("} else {");
                    indent++;
                    Append(Stringify(ce.IfFalse, indent));
                }

                indent--;
                Append("}");
            }
            else if (exp is SwitchExpression se)
            {
                Append($"switch ({Substring(se.SwitchValue)}) {{");
                indent++;

                foreach (var cas in se.Cases)
                {
                    Append("case " + string.Join(", ", cas.TestValues.Select(t => Stringify(t))) + ": {");
                    indent++;
                    Append(Substring(cas.Body));
                    Append("break;");
                    indent--;
                    Append("}");
                }

                indent--;
                Append("}");
            }
            else if (exp is ConstantExpression con)
            {
                if (con.Type == typeof(char)) return $"'{con.Value}'";
                if (con.Type == typeof(string)) return $"\"{con.Value}\"";
                if (con.Type == typeof(bool)) return (bool)con.Value ? "true" : "false";
                if (con.Type.IsEnum) return con.Type.FullName + '.' + con.Value;
                Append(con.ToString());
            }
            else if (exp is DefaultExpression def)
            {
                if (def.Type != typeof(void))
                {
                    Append(def.ToString());
                }
            }
            else if (exp is BinaryExpression bin)
            {
                if (bin.NodeType == ExpressionType.ArrayIndex)
                {
                    return Stringify(bin.Left) +
                        "[" + Stringify(bin.Right) + "]";
                }

                return string.Join(" ",
                    Stringify(bin.Left),
                    _expressionTypelookup[bin.NodeType],
                    Stringify(bin.Right));
            }
            else if (exp is UnaryExpression unary)
            {
                if (unary.NodeType == ExpressionType.ArrayLength) return $"{Stringify(unary.Operand)}.Length";
                if (unary.NodeType == ExpressionType.Not) return $"!({Stringify(unary.Operand)})";
                if (unary.Method.IsStatic && unary.Method.DeclaringType != null)
                {
                    return unary.Method.DeclaringType.Name + "(" + Stringify(unary.Operand) + ")";
                }
            }
            else if (exp is MethodCallExpression call)
            {
                if (call.Method.IsStatic && call.Method.DeclaringType != null)
                {
                    return call.Method.DeclaringType.Name + "." +
                        call.Method.Name + "(" +
                        string.Join(", ", call.Arguments.Select(t => Stringify(t))) + ")";
                }
            }
            else if (exp is ParameterExpression typedParam)
            {
                return typedParam.Name;
            }
            else if (exp is LabelExpression label)
            {
                // Ew lol
                return "return default;";
            }
            else if (exp is MemberExpression member)
            {
                return member.ToString();
            }
            else if (exp is IndexExpression index)
            {
                return Stringify(index.Object) + "[" + Stringify(index.Arguments[0]) + "]";
            }
            else
            {
                Append(exp.ToString());
            }

            return sb.ToString().Trim();
        }

        private static readonly Dictionary<ExpressionType, string> _expressionTypelookup = new() {
            { ExpressionType.GreaterThanOrEqual, ">=" },
            { ExpressionType.GreaterThan, ">" },
            { ExpressionType.LessThanOrEqual, "<=" },
            { ExpressionType.LessThan, "<" },
            { ExpressionType.Equal, "==" },
            { ExpressionType.NotEqual, "!=" },
            { ExpressionType.And, "&" },
            { ExpressionType.AndAlso, "&&" },
            { ExpressionType.Or, "|" },
            { ExpressionType.OrElse, "||" },
        };
    }
}
