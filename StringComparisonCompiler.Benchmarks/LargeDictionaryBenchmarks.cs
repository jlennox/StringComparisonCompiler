using System;
using BenchmarkDotNet.Attributes;

namespace StringComparisonCompiler.Benchmarks
{
    public class LargeDictionaryBenchmarks
    {
        private static readonly StringComparisonCompiler<KeywordsEnum>.SpanStringComparer _compiled = StringComparisonCompiler<KeywordsEnum>.CompileSpan();
        private static readonly MatchTree<KeywordsEnum> _trie = new(StringComparison.CurrentCulture, false);

        [Params("while", "foreach", "stackalloc", "DoesNotExist")]
        public string N;

        [Benchmark]
        public KeywordsEnum Compiled()
        {
            return _compiled(N);
        }

        [Benchmark]
        public bool Trie()
        {
            return _trie.ContainsWord(N);
        }

        [Benchmark]
        public KeywordsEnum Switch()
        {
            switch (N)
            {
                case "abstract": return KeywordsEnum.Abstract;
                case "as": return KeywordsEnum.As;
                case "base": return KeywordsEnum.Base;
                case "bool": return KeywordsEnum.Bool;
                case "break": return KeywordsEnum.Break;
                case "byte": return KeywordsEnum.Byte;
                case "case": return KeywordsEnum.Case;
                case "catch": return KeywordsEnum.Catch;
                case "char": return KeywordsEnum.Char;
                case "checked": return KeywordsEnum.Checked;
                case "class": return KeywordsEnum.Class;
                case "const": return KeywordsEnum.Const;
                case "continue": return KeywordsEnum.Continue;
                case "decimal": return KeywordsEnum.Decimal;
                case "default": return KeywordsEnum.Default;
                case "delegate": return KeywordsEnum.Delegate;
                case "do": return KeywordsEnum.Do;
                case "double": return KeywordsEnum.Double;
                case "else": return KeywordsEnum.Else;
                case "enum": return KeywordsEnum.Enum;
                case "event": return KeywordsEnum.Event;
                case "explicit": return KeywordsEnum.Explicit;
                case "extern": return KeywordsEnum.Extern;
                case "false": return KeywordsEnum.False;
                case "finally": return KeywordsEnum.Finally;
                case "fixed": return KeywordsEnum.Fixed;
                case "float": return KeywordsEnum.Float;
                case "for": return KeywordsEnum.For;
                case "foreach": return KeywordsEnum.Foreach;
                case "goto": return KeywordsEnum.Goto;
                case "if": return KeywordsEnum.If;
                case "implicit": return KeywordsEnum.Implicit;
                case "in": return KeywordsEnum.In;
                case "int": return KeywordsEnum.Int;
                case "interface": return KeywordsEnum.Interface;
                case "internal": return KeywordsEnum.Internal;
                case "is": return KeywordsEnum.Is;
                case "lock": return KeywordsEnum.Lock;
                case "long": return KeywordsEnum.Long;
                case "namespace": return KeywordsEnum.Namespace;
                case "new": return KeywordsEnum.New;
                case "null": return KeywordsEnum.Null;
                case "object": return KeywordsEnum.Object;
                case "operator": return KeywordsEnum.Operator;
                case "out": return KeywordsEnum.Out;
                case "override": return KeywordsEnum.Override;
                case "params": return KeywordsEnum.Params;
                case "private": return KeywordsEnum.Private;
                case "protected": return KeywordsEnum.Protected;
                case "public": return KeywordsEnum.Public;
                case "readonly": return KeywordsEnum.Readonly;
                case "ref": return KeywordsEnum.Ref;
                case "return": return KeywordsEnum.Return;
                case "sbyte": return KeywordsEnum.Sbyte;
                case "sealed": return KeywordsEnum.Sealed;
                case "short": return KeywordsEnum.Short;
                case "sizeof": return KeywordsEnum.Sizeof;
                case "stackalloc": return KeywordsEnum.Stackalloc;
                case "static": return KeywordsEnum.Static;
                case "string": return KeywordsEnum.String;
                case "struct": return KeywordsEnum.Struct;
                case "switch": return KeywordsEnum.Switch;
                case "this": return KeywordsEnum.This;
                case "throw": return KeywordsEnum.Throw;
                case "true": return KeywordsEnum.True;
                case "try": return KeywordsEnum.Try;
                case "typeof": return KeywordsEnum.Typeof;
                case "uint": return KeywordsEnum.Uint;
                case "ulong": return KeywordsEnum.Ulong;
                case "unchecked": return KeywordsEnum.Unchecked;
                case "unsafe": return KeywordsEnum.Unsafe;
                case "ushort": return KeywordsEnum.Ushort;
                case "using": return KeywordsEnum.Using;
                case "virtual": return KeywordsEnum.Virtual;
                case "void": return KeywordsEnum.Void;
                case "volatile": return KeywordsEnum.Volatile;
                case "while": return KeywordsEnum.While;
                default: return KeywordsEnum.Abstract;
            };
        }

        [Benchmark]
        public KeywordsEnum SwitchExpression()
        {
            return N switch
            {
                "abstract" => KeywordsEnum.Abstract,
                "as" => KeywordsEnum.As,
                "base" => KeywordsEnum.Base,
                "bool" => KeywordsEnum.Bool,
                "break" => KeywordsEnum.Break,
                "byte" => KeywordsEnum.Byte,
                "case" => KeywordsEnum.Case,
                "catch" => KeywordsEnum.Catch,
                "char" => KeywordsEnum.Char,
                "checked" => KeywordsEnum.Checked,
                "class" => KeywordsEnum.Class,
                "const" => KeywordsEnum.Const,
                "continue" => KeywordsEnum.Continue,
                "decimal" => KeywordsEnum.Decimal,
                "default" => KeywordsEnum.Default,
                "delegate" => KeywordsEnum.Delegate,
                "do" => KeywordsEnum.Do,
                "double" => KeywordsEnum.Double,
                "else" => KeywordsEnum.Else,
                "enum" => KeywordsEnum.Enum,
                "event" => KeywordsEnum.Event,
                "explicit" => KeywordsEnum.Explicit,
                "extern" => KeywordsEnum.Extern,
                "false" => KeywordsEnum.False,
                "finally" => KeywordsEnum.Finally,
                "fixed" => KeywordsEnum.Fixed,
                "float" => KeywordsEnum.Float,
                "for" => KeywordsEnum.For,
                "foreach" => KeywordsEnum.Foreach,
                "goto" => KeywordsEnum.Goto,
                "if" => KeywordsEnum.If,
                "implicit" => KeywordsEnum.Implicit,
                "in" => KeywordsEnum.In,
                "int" => KeywordsEnum.Int,
                "interface" => KeywordsEnum.Interface,
                "internal" => KeywordsEnum.Internal,
                "is" => KeywordsEnum.Is,
                "lock" => KeywordsEnum.Lock,
                "long" => KeywordsEnum.Long,
                "namespace" => KeywordsEnum.Namespace,
                "new" => KeywordsEnum.New,
                "null" => KeywordsEnum.Null,
                "object" => KeywordsEnum.Object,
                "operator" => KeywordsEnum.Operator,
                "out" => KeywordsEnum.Out,
                "override" => KeywordsEnum.Override,
                "params" => KeywordsEnum.Params,
                "private" => KeywordsEnum.Private,
                "protected" => KeywordsEnum.Protected,
                "public" => KeywordsEnum.Public,
                "readonly" => KeywordsEnum.Readonly,
                "ref" => KeywordsEnum.Ref,
                "return" => KeywordsEnum.Return,
                "sbyte" => KeywordsEnum.Sbyte,
                "sealed" => KeywordsEnum.Sealed,
                "short" => KeywordsEnum.Short,
                "sizeof" => KeywordsEnum.Sizeof,
                "stackalloc" => KeywordsEnum.Stackalloc,
                "static" => KeywordsEnum.Static,
                "string" => KeywordsEnum.String,
                "struct" => KeywordsEnum.Struct,
                "switch" => KeywordsEnum.Switch,
                "this" => KeywordsEnum.This,
                "throw" => KeywordsEnum.Throw,
                "true" => KeywordsEnum.True,
                "try" => KeywordsEnum.Try,
                "typeof" => KeywordsEnum.Typeof,
                "uint" => KeywordsEnum.Uint,
                "ulong" => KeywordsEnum.Ulong,
                "unchecked" => KeywordsEnum.Unchecked,
                "unsafe" => KeywordsEnum.Unsafe,
                "ushort" => KeywordsEnum.Ushort,
                "using" => KeywordsEnum.Using,
                "virtual" => KeywordsEnum.Virtual,
                "void" => KeywordsEnum.Void,
                "volatile" => KeywordsEnum.Volatile,
                "while" => KeywordsEnum.While,
                _ => KeywordsEnum.Abstract,
            };
        }

        [Benchmark]
        public KeywordsEnum If()
        {
            if (N == "abstract") return KeywordsEnum.Abstract;
            if (N == "as") return KeywordsEnum.As;
            if (N == "base") return KeywordsEnum.Base;
            if (N == "bool") return KeywordsEnum.Bool;
            if (N == "break") return KeywordsEnum.Break;
            if (N == "byte") return KeywordsEnum.Byte;
            if (N == "case") return KeywordsEnum.Case;
            if (N == "catch") return KeywordsEnum.Catch;
            if (N == "char") return KeywordsEnum.Char;
            if (N == "checked") return KeywordsEnum.Checked;
            if (N == "class") return KeywordsEnum.Class;
            if (N == "const") return KeywordsEnum.Const;
            if (N == "continue") return KeywordsEnum.Continue;
            if (N == "decimal") return KeywordsEnum.Decimal;
            if (N == "default") return KeywordsEnum.Default;
            if (N == "delegate") return KeywordsEnum.Delegate;
            if (N == "do") return KeywordsEnum.Do;
            if (N == "double") return KeywordsEnum.Double;
            if (N == "else") return KeywordsEnum.Else;
            if (N == "enum") return KeywordsEnum.Enum;
            if (N == "event") return KeywordsEnum.Event;
            if (N == "explicit") return KeywordsEnum.Explicit;
            if (N == "extern") return KeywordsEnum.Extern;
            if (N == "false") return KeywordsEnum.False;
            if (N == "finally") return KeywordsEnum.Finally;
            if (N == "fixed") return KeywordsEnum.Fixed;
            if (N == "float") return KeywordsEnum.Float;
            if (N == "for") return KeywordsEnum.For;
            if (N == "foreach") return KeywordsEnum.Foreach;
            if (N == "goto") return KeywordsEnum.Goto;
            if (N == "if") return KeywordsEnum.If;
            if (N == "implicit") return KeywordsEnum.Implicit;
            if (N == "in") return KeywordsEnum.In;
            if (N == "int") return KeywordsEnum.Int;
            if (N == "interface") return KeywordsEnum.Interface;
            if (N == "internal") return KeywordsEnum.Internal;
            if (N == "is") return KeywordsEnum.Is;
            if (N == "lock") return KeywordsEnum.Lock;
            if (N == "long") return KeywordsEnum.Long;
            if (N == "namespace") return KeywordsEnum.Namespace;
            if (N == "new") return KeywordsEnum.New;
            if (N == "null") return KeywordsEnum.Null;
            if (N == "object") return KeywordsEnum.Object;
            if (N == "operator") return KeywordsEnum.Operator;
            if (N == "out") return KeywordsEnum.Out;
            if (N == "override") return KeywordsEnum.Override;
            if (N == "params") return KeywordsEnum.Params;
            if (N == "private") return KeywordsEnum.Private;
            if (N == "protected") return KeywordsEnum.Protected;
            if (N == "public") return KeywordsEnum.Public;
            if (N == "readonly") return KeywordsEnum.Readonly;
            if (N == "ref") return KeywordsEnum.Ref;
            if (N == "return") return KeywordsEnum.Return;
            if (N == "sbyte") return KeywordsEnum.Sbyte;
            if (N == "sealed") return KeywordsEnum.Sealed;
            if (N == "short") return KeywordsEnum.Short;
            if (N == "sizeof") return KeywordsEnum.Sizeof;
            if (N == "stackalloc") return KeywordsEnum.Stackalloc;
            if (N == "static") return KeywordsEnum.Static;
            if (N == "string") return KeywordsEnum.String;
            if (N == "struct") return KeywordsEnum.Struct;
            if (N == "switch") return KeywordsEnum.Switch;
            if (N == "this") return KeywordsEnum.This;
            if (N == "throw") return KeywordsEnum.Throw;
            if (N == "true") return KeywordsEnum.True;
            if (N == "try") return KeywordsEnum.Try;
            if (N == "typeof") return KeywordsEnum.Typeof;
            if (N == "uint") return KeywordsEnum.Uint;
            if (N == "ulong") return KeywordsEnum.Ulong;
            if (N == "unchecked") return KeywordsEnum.Unchecked;
            if (N == "unsafe") return KeywordsEnum.Unsafe;
            if (N == "ushort") return KeywordsEnum.Ushort;
            if (N == "using") return KeywordsEnum.Using;
            if (N == "virtual") return KeywordsEnum.Virtual;
            if (N == "void") return KeywordsEnum.Void;
            if (N == "volatile") return KeywordsEnum.Volatile;
            if (N == "while") return KeywordsEnum.While;
            return KeywordsEnum.Abstract;
        }
    }
}
