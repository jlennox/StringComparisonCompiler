using System;
using System.Runtime.CompilerServices;

namespace StringComparisonCompiler
{
    internal static class SpanHelper
    {
        // Work around for https://github.com/dotnet/runtime/issues/24621
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once UnusedMember.Local
        internal static char GetChar(ReadOnlySpan<char> span, int index)
        {
            return span[index];
        }
    }
}