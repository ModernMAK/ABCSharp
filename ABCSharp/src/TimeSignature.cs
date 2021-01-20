namespace ABCSharp
{
    public struct TimeSignature
    {
        public TimeSignature(Fraction meter, Fraction unitLength, object tempo)
        {
            Meter = meter;
            UnitLength = unitLength;
            Tenpo = tempo;
        }

        public bool FreeMeeter => Meter.Equals(default(Fraction));
        public Fraction Meter { get; set; }
        public Fraction UnitLength { get; set; }

        public Fraction AsUnitLength(Fraction fraction) => fraction * UnitLength;
        public Fraction AsBeat(Fraction fraction) => fraction * Meter.Denominator; //divide by 1/denom
        public static Fraction AsBeat(Fraction fraction, int beatNote) => fraction * beatNote;
        public Fraction ConvertToMeter(Fraction fraction) => fraction * Meter;
        public object Tenpo { get; set; }

        public static Fraction ParseFraction(string fraction)
        {
            
            var split = fraction.Split('/');
            var n =int.Parse(split[0]);
            var d = int.Parse(split[1]);
            return new Fraction(n,d);
        }

        public static Fraction ParseMeter(string meter)
        {
            meter = meter.Replace("C|", "2/2").Replace("C", "4/4");
            if (meter == "free")
                return default;
            else
                return ParseFraction(meter);
        }
    }
}