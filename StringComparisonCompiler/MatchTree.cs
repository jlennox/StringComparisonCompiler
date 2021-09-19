using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace StringComparisonCompiler
{
    internal class MatchTree
    {
        protected readonly MethodInfo? CharTransform;

        private readonly MatchNode<long> _tree;

        public MatchTree(string[] input, StringComparison comparison)
            : this(comparison)
        {
            var lookup = CreateArrayLookup(input);
            _tree = CreateTree(lookup);
        }

        protected MatchTree(StringComparison comparison)
        {
            CharTransform = GetCharTransform(comparison);
            _tree = default;
        }

        // I hate this lack of code re-use on Compile.
        public virtual TReturnType Compile<TReturnType>(
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
            var returnLabel = Expression.Label(typeof(long));

            expression = Expression.Block(new[] {
                _tree.Compile(returnLabel, parameter, inputType, CharTransform),
                Expression.Label(returnLabel, MatchNodeCompiler<long>.DefaultResult)
            });

            return Expression.Lambda<TReturnType>(expression, parameter).Compile(false);
        }

        protected static MethodInfo? GetCharTransform(StringComparison comparison)
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

        internal static IReadOnlyDictionary<string, long> CreateArrayLookup(string[] input)
        {
            var result = new Dictionary<string, long>();
            for (var i = 0; i < input.Length; ++i) result[input[i]] = i;
            return result;
        }

        protected MatchNode<T> CreateTree<T>(IReadOnlyDictionary<string, T> lookup)
        {
            var tree = new MatchNode<T>('\x0');

            foreach (var entry in lookup)
            {
                var word = entry.Key;
                var val = entry.Value;
                var node = tree;

                for (var i = 0; i < word.Length; ++i)
                {
                    var chr = CharTransform != null
                        ? (char)CharTransform.Invoke(null, new object[] { word[i] })
                        : word[i];

                    var isTerminal = i == word.Length - 1;
                    node = node.GetChildOrCreate(chr, isTerminal, isTerminal ? val : default);
                }
            }

            return tree;
        }
    }

    internal sealed class MatchTree<TEnum> : MatchTree
        where TEnum : struct, Enum
    {
        private readonly MatchNode<TEnum> _tree;

        public MatchTree(StringComparison comparison)
            : base(comparison)
        {
            var lookup = CreateEnumLookup();
            _tree = CreateTree(lookup);
        }

        public override TReturnType Compile<TReturnType>(
            MatchNodeCompilerInputType inputType,
            out Expression expression)
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
                _tree.Compile(returnLabel, parameter, inputType, CharTransform),
                Expression.Label(returnLabel, MatchNodeCompiler<TEnum>.DefaultResult)
            });

            return Expression.Lambda<TReturnType>(expression, parameter).Compile(false);
        }

        internal static IReadOnlyDictionary<string, TEnum> CreateEnumLookup()
        {
            var fields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);
            var result = new Dictionary<string, TEnum>();

            foreach (var field in fields)
            {
                var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>(false);
                var name = descriptionAttr?.Description ?? field.Name;

#if NET471
                if (result.ContainsKey(name))
                {
                    throw new ArgumentException($"Duplicate key with name '{name}' was encountered.", nameof(TEnum));
                }

                result[name] = (TEnum)Enum.Parse(typeof(TEnum), field.Name);
#else
                if (!result.TryAdd(name, Enum.Parse<TEnum>(field.Name)))
                {
                    throw new ArgumentException($"Duplicate key with name '{name}' was encountered.", nameof(TEnum));
                }
#endif
            }

            return result;
        }

        // Exists for profiling purposes, to compare the compiled vs the soft execution speeds.
        internal bool ContainsWord(string word)
        {
            return _tree.ContainsTerminal(word, CharTransform);
        }

        // For debug purposes.
        // ReSharper disable once UnusedMember.Global
        internal string GetDescription()
        {
            return _tree.GetDescription();
        }
    }
}
