About
----

Nuget: https://www.nuget.org/packages/StringComparisonCompiler

StringComparisonCompiler is an optimizing compiler for string comparison. This replaces if/while comparisons, which can
be performantly slow, with an dynamically compiled unrolled comparison. Case insensitive compares are exceptionally
more performant.

The slow pattern:

```C#
while (inputString) {
    case "foo": ...
    case "bar": ...
    case "baz": ...
}
```

The code StringComparisonCompiler generates (returns matching index, or -1):
```C#
if (s.Length != 3) {
    return -1;
}

switch (s[0]) {
    case 'f': {
        return (s[1] == 'o' && s[2] == 'o')
            ? (0)
            : (-1);
        break;
    }
    case 'b': {
        switch (s[1]) {
            case 'a': {
                switch (s[2]) {
                    case 'r': {
                        return 1;
                        break;
                    }
                    case 'z': {
                        return 2;
                        break;
                    }
                }
                break;
            }
        }
        break;
    }
}
return -1;
```


Usage
----
Basic usage with array input returning found index:
```C#
var arr = new[] { "foo", "bar", "baz" };
var compiled = StringComparisonCompiler.Compile(arr);
Assert.AreEqual(0, compiled("foo"));
Assert.AreEqual(1, compiled("bar"));
Assert.AreEqual(2, compiled("baz"));
Assert.AreEqual(-1, compiled("Not found"));
```

`CompileSpan` can be called to compile a function that accepts **`ReadOnlySpan<char>`** but note that there is a performance
penalty for using `ReadOnlySpan<char>` due non-removable index checks (theory).

**Case insensitivity** can be set by passing `Compile` a `StringComparison`.

**Enums** can also be used and returned. The `System.ComponentModel.Description` attribute can be used to map the enum entry
to a different input.

```C#
public enum Foobar {
    Default,
    [Description("Renamed Entry")]
    Foo,
    Bar,
    Baz,
}

public void Test()
{
    var compiled = StringComparisonCompiler<Foobar>.Compile();

    Assert.AreEqual(Foobar.Default, compiled("Default"));
    Assert.AreEqual(Foobar.Foo, compiled("Renamed Entry"));
    Assert.AreEqual(Foobar.Bar, compiled("Bar"));
    Assert.AreEqual(Foobar.Baz, compiled("Baz"));

    Assert.AreEqual(Foobar.Default, compiled("Foo"));
    Assert.AreEqual(Foobar.Default, compiled("Foobar"));
    Assert.AreEqual(Foobar.Default, compiled("Not a realy entry!"));
}
```

Future development
---
Compile time code generation would be better for non-dynamic inputs such as enums, as it doesn't consume runtime CPU.

Linq Expressions sadly does not allow for "unsafe" code, which makes the performs of ReadOnlySpan<char> extremely poor,
and prevents folding long single path runs in strings down to short, int, long comparisons. It's very possible even when
it's not a folded path, that doing larger comparisons are better even if they don't merge paths.

Benchmarks
---

### CaseInsensitiveBenchmark
```
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.18363.1801 (1909/November2019Update/19H2)
Intel Core i9-10900K CPU 3.70GHz, 1 CPU, 20 logical and 10 physical cores
.NET SDK=6.0.100-preview.2.21155.3
  [Host]     : .NET 5.0.9 (5.0.921.35908), X64 RyuJIT
  DefaultJob : .NET 5.0.9 (5.0.921.35908), X64 RyuJIT


|       Method |            N |        Mean |     Error |    StdDev |
|------------- |------------- |------------:|----------:|----------:|
| CompiledSpan | DoesNotExist |   1.7207 ns | 0.0018 ns | 0.0016 ns |
|     Compiled | DoesNotExist |   0.6302 ns | 0.0018 ns | 0.0017 ns |
|         Trie | DoesNotExist |  19.0213 ns | 0.0180 ns | 0.0168 ns |
|       Switch | DoesNotExist |  25.2402 ns | 0.1657 ns | 0.1550 ns |
|           If | DoesNotExist | 164.7618 ns | 0.2894 ns | 0.2259 ns |
| CompiledSpan |       Foobar |   6.3321 ns | 0.0262 ns | 0.0245 ns |
|     Compiled |       Foobar |   6.1435 ns | 0.0035 ns | 0.0033 ns |
|         Trie |       Foobar |  20.5932 ns | 0.0212 ns | 0.0188 ns |
|       Switch |       Foobar |  24.7442 ns | 0.1396 ns | 0.1306 ns |
|           If |       Foobar | 177.5240 ns | 0.1351 ns | 0.1128 ns |
| CompiledSpan |      ForEach |  12.3496 ns | 0.0402 ns | 0.0376 ns |
|     Compiled |      ForEach |  12.3390 ns | 0.0099 ns | 0.0088 ns |
|         Trie |      ForEach |  54.0590 ns | 0.0332 ns | 0.0277 ns |
|       Switch |      ForEach |  20.5421 ns | 0.1072 ns | 0.1003 ns |
|           If |      ForEach | 128.9901 ns | 0.2565 ns | 0.2399 ns |
| CompiledSpan |        While |   9.4120 ns | 0.0068 ns | 0.0064 ns |
|     Compiled |        While |   9.2554 ns | 0.0169 ns | 0.0158 ns |
|         Trie |        While |  36.6725 ns | 0.0429 ns | 0.0358 ns |
|       Switch |        While |  23.4200 ns | 0.1073 ns | 0.1003 ns |
|           If |        While | 175.7481 ns | 0.2655 ns | 0.2354 ns |
```

### LargeDictionaryBenchmarks

```
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.18363.1801 (1909/November2019Update/19H2)
Intel Core i9-10900K CPU 3.70GHz, 1 CPU, 20 logical and 10 physical cores
.NET SDK=6.0.100-rc.1.21458.32
  [Host]     : .NET 5.0.9 (5.0.921.35908), X64 RyuJIT
  DefaultJob : .NET 5.0.9 (5.0.921.35908), X64 RyuJIT


|           Method |            N |       Mean |     Error |    StdDev |
|----------------- |------------- |-----------:|----------:|----------:|
|     CompiledSpan | DoesNotExist |  38.461 ns | 0.1385 ns | 0.1228 ns |
|         Compiled | DoesNotExist |   1.870 ns | 0.0099 ns | 0.0087 ns |
|    CompiledArray | DoesNotExist |   2.248 ns | 0.0050 ns | 0.0044 ns |
|             Trie | DoesNotExist |   7.287 ns | 0.0129 ns | 0.0121 ns |
|           Switch | DoesNotExist |   7.322 ns | 0.0146 ns | 0.0114 ns |
| SwitchExpression | DoesNotExist |   7.511 ns | 0.0178 ns | 0.0167 ns |
|               If | DoesNotExist | 162.986 ns | 0.2508 ns | 0.2223 ns |
|     CompiledSpan |         base |  39.593 ns | 0.0591 ns | 0.0524 ns |
|         Compiled |         base |   3.608 ns | 0.0067 ns | 0.0056 ns |
|    CompiledArray |         base |   4.137 ns | 0.0143 ns | 0.0133 ns |
|             Trie |         base |  30.772 ns | 0.0457 ns | 0.0382 ns |
|           Switch |         base |   4.383 ns | 0.0188 ns | 0.0167 ns |
| SwitchExpression |         base |   4.886 ns | 0.0340 ns | 0.0318 ns |
|               If |         base |   4.808 ns | 0.0250 ns | 0.0209 ns |
|     CompiledSpan |      foreach |  41.914 ns | 0.0968 ns | 0.0756 ns |
|         Compiled |      foreach |   4.371 ns | 0.0162 ns | 0.0143 ns |
|    CompiledArray |      foreach |   4.834 ns | 0.0203 ns | 0.0190 ns |
|             Trie |      foreach |  54.981 ns | 0.0858 ns | 0.0761 ns |
|           Switch |      foreach |   7.098 ns | 0.0229 ns | 0.0214 ns |
| SwitchExpression |      foreach |   6.277 ns | 0.0178 ns | 0.0166 ns |
|               If |      foreach |  72.353 ns | 0.1049 ns | 0.0982 ns |
|     CompiledSpan |   stackalloc |  43.678 ns | 0.0925 ns | 0.0865 ns |
|         Compiled |   stackalloc |   5.203 ns | 0.0309 ns | 0.0289 ns |
|    CompiledArray |   stackalloc |   5.175 ns | 0.0061 ns | 0.0054 ns |
|             Trie |   stackalloc |  81.699 ns | 0.0942 ns | 0.0881 ns |
|           Switch |   stackalloc |   7.772 ns | 0.0359 ns | 0.0319 ns |
| SwitchExpression |   stackalloc |   7.865 ns | 0.0466 ns | 0.0436 ns |
|               If |   stackalloc | 122.800 ns | 0.1243 ns | 0.1163 ns |
|     CompiledSpan |         void |  40.141 ns | 0.0525 ns | 0.0491 ns |
|         Compiled |         void |   3.493 ns | 0.0082 ns | 0.0073 ns |
|    CompiledArray |         void |   3.709 ns | 0.0059 ns | 0.0055 ns |
|             Trie |         void |  33.673 ns | 0.0382 ns | 0.0358 ns |
|           Switch |         void |   4.999 ns | 0.0380 ns | 0.0356 ns |
| SwitchExpression |         void |   4.839 ns | 0.0282 ns | 0.0236 ns |
|               If |         void | 198.194 ns | 0.3763 ns | 0.3336 ns |
```