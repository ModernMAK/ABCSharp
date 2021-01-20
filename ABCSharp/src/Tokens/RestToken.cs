namespace ABCSharp.Tokens
{
    public class RestToken : Token
    {
        public string Symbol { get; set; }
        public Fraction Length { get; set; }
        public TimeSignature TimeSignature { get; set; }
		public bool MeasureRest { get; internal set; }

		public void Dotify(string dots, string direction)
        {
            Length = DurationHelper.Dotify(Length, dots, direction);
        }
    }
}