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

        public Fraction Meter { get; set; }
        public Fraction UnitLength { get; set; }
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