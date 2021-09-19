using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace StringComparisonCompiler
{
    internal class MatchTree<TEnum>
        where TEnum : struct, Enum
    {
        public Expression? Exp;

        private readonly MethodInfo? _charTransform;
        private readonly bool _testStartsWith;
        private readonly MatchNode<TEnum> _tree = new('\x0');

        public MatchTree(StringComparison comparison, bool testStartsWith)
        {
            _charTransform = GetCharTransform(comparison);
            _testStartsWith = testStartsWith;

            var lookup = CreateLookup();
            foreach (var (word, val) in lookup)
            {
                var node = _tree;

                for (var i = 0; i < word.Length; ++i)
                {
                    var chr = _charTransform != null
                        ? (char)_charTransform.Invoke(null, new object[] { word[i] })
                        : word[i];

                    var isTerminal = i == word.Length - 1;
                    node = node.GetChildOrCreate(chr, isTerminal, isTerminal ? val : default);
                }
            }
        }

        public TReturnType Compile<TReturnType>(
            MatchNodeCompilerInputType inputType,
            out Expression expression) where TReturnType : Delegate
        {
            var parameterType = inputType switch
            {
                MatchNodeCompilerInputType.String => typeof(string),
                MatchNodeCompilerInputType.CharSpan => typeof(ReadOnlySpan<char>),
                _ => throw new NotImplementedException(),
            };

            var parameter = Expression.Parameter(parameterType, "s");
            var returnLabel = Expression.Label(typeof(TEnum));

            expression = Expression.Block(new[] {
                _tree.Compile(returnLabel, parameter, inputType, _testStartsWith, _charTransform),
                Expression.Label(returnLabel, MatchNodeCompiler<TEnum>.DefaultResult)
            });

            Exp = expression;
            return Expression.Lambda<TReturnType>(expression, parameter).Compile(false);
        }

        // For debug purposes.
        internal string GetDescription()
        {
            return _tree.GetDescription();
        }

        internal static IReadOnlyDictionary<string, TEnum> CreateLookup()
        {
            var fields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);
            var result = new Dictionary<string, TEnum>();

            foreach (var field in fields)
            {
                var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>(false);
                var name = descriptionAttr?.Description ?? field.Name;

                if (!result.TryAdd(name, Enum.Parse<TEnum>(field.Name)))
                {
                    throw new ArgumentException($"Duplicate key with name '{name}' was encountered.", nameof(TEnum));
                }
            }

            return result;
        }

        private static MethodInfo? GetCharTransform(StringComparison comparison)
        {
            var ignoreCulture = comparison switch
            {
                StringComparison.InvariantCulture => true,
                StringComparison.InvariantCultureIgnoreCase => true,
                StringComparison.OrdinalIgnoreCase => true,
                _ => false
            };

            var ignoreCase = comparison switch
            {
                StringComparison.CurrentCultureIgnoreCase => true,
                StringComparison.InvariantCultureIgnoreCase => true,
                StringComparison.OrdinalIgnoreCase => true,
                _ => false
            };

            return (ignoreCase, ignoreCulture) switch
            {
                (true, true) => typeof(char).GetMethod(
                    nameof(char.ToUpperInvariant),
                    new[] { typeof(char) }, null),
                // Arg. This should have something...
                (false, true) => null,
                (true, false) => typeof(char).GetMethod(
                    nameof(char.ToUpper),
                    new[] { typeof(char) }, null),
                (false, false) => null
            };
        }

        // Exists for profiling purposes, to compare the compiled vs the soft execution speeds.
        internal bool ContainsWord(string word)
        {
            return _tree.ContainsTerminal(word, _charTransform);
        }
    }
}
