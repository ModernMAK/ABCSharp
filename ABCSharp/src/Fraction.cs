namespace ABCSharp
{
    public struct Fraction
    {
		public Fraction(Fraction length) : this()
		{
            Numerator = length.Numerator;
            Denominator = length.Denominator;
		}

		public Fraction(int n, int d)
        {
            Numerator = n;
            Denominator = d;
        }

        public int Numerator { get; }
        public int Denominator { get; }
        public float Value => (float) Numerator / Denominator;

        public static Fraction operator *(Fraction a, Fraction b) => new Fraction(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
        public static Fraction operator /(Fraction a, Fraction b) => new Fraction(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
        public static Fraction operator *(Fraction a, int b) => new Fraction(a.Numerator * b, a.Denominator);
        public static Fraction operator /(Fraction a, int b) => new Fraction(a.Numerator, a.Denominator * b);
    }
}