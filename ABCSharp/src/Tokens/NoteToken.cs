using System;

namespace ABCSharp.Tokens
{
    public static class DurationHelper
    {
        public static Fraction Dotify(Fraction fraction, string dots, string direction)
        {
            var longer = (direction == "left");
            if (dots.Contains("<"))
                longer = !longer;
            var l = fraction;
            return longer
                ? new Fraction(l.Numerator * 2 + 1, l.Denominator * 2)
                : new Fraction(l.Numerator, l.Denominator * 2);
        }
    }

    public class NoteToken : Token
    {
        public Key Key { get; set; }
        public TimeSignature TimeSignature { get; set; }
        public string Note { get; set; }
        public string Accidental { get; set; }

        public int Octave { get; set; }

        public Fraction Length { get; set; }

        [Obsolete("Redundant; use Length.Value")]
        public float Duration => Length.Value;

        public Pitch Pitch { get; }

        public void Dotify(string dots, string direction)
        {
            Length = DurationHelper.Dotify(Length, dots, direction);
        }
    }
}