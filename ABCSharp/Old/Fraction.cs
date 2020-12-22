using System.ComponentModel;
using System.IO;

namespace ABCSharp.Old
{
    public struct Fraction
    {
        public int Numerator { get; }
        public int Denominator { get; }
        public float Value => (float)Numerator / Denominator;
    }
}