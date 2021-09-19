namespace StringComparisonCompiler
{
    internal readonly struct MinMax
    {
        public int Min { get; }
        public int Max { get; }

        public MinMax(int min, int max)
        {
            Min = min;
            Max = max;
        }
        public override bool Equals(object? obj)
        {
            return obj is MinMax other && Equals(other);
        }

        public bool Equals(MinMax other)
        {
            return Min == other.Min && Max == other.Max;
        }

        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }

        public static bool operator ==(MinMax left, MinMax right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MinMax left, MinMax right)
        {
            return !left.Equals(right);
        }
    }
}