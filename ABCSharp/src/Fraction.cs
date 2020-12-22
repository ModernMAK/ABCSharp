namespace ABCSharp
{
    public struct Fraction
    {
        public Fraction(int n, int d)
        {
            Numerator = n;
            Denominator = d;
        }

        public int Numerator { get; }
        public int Denominator { get; }
        public float Value => (float) Numerator / Denominator;
    }
}