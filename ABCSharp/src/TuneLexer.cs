using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ABCSharp.Tokens;

namespace ABCSharp
{
    public class TuneLexer
    {
        public TuneLexer()
        {
            Tokens = new List<Token>();
        }

        private List<Token> Tokens { get; }
        private string PendingDots { get; set; }
        private bool HasPendingDots => PendingDots != null;
        private Key Key { get; set; }
        private TimeSignature TimeSignature { get; set; }

        public bool TryLexHeader(string line, int lineIndex)
        {
            if (line.Length <= 2 || (!InfoFields.IsHeader(line, InfoFields.TuneBodyInfoKeys) &&
                                     !InfoFields.IsHeaderContinuation(line))) return false;

            var bodyFieldToken = new BodyFieldToken() {Line = lineIndex, Char = 0, Text = line};
            Tokens.Add(bodyFieldToken);
            return true;
        }


        private static readonly string InlineFields = new string(InfoFields.InlineInfoKeys.Keys.ToArray());
        private static readonly Regex InlineRegex = new Regex($@"^\[([{InlineFields}]:)([^\]]+)\]");

        public bool TryLexInlineField(string line, int lineIndex, ref int charIndex)
        {
            var match = InlineRegex.Match(line);
            if (!match.Success) return false;

            var header = match.Groups[1].Value;
            var text = match.Groups[2].Value;

            //Original didn't check for "K:" just "K" so this may fail
            //Since its not a 1-1 translation
            if (InfoFields.IsHeader(header, InfoFields.Key))
            {
                Key = Key.FromName(text);
            }

            var inlineFieldToken = new InlineFieldToken()
                {Line = lineIndex, Char = charIndex, Text = match.Value};
            Tokens.Add(inlineFieldToken);
            charIndex += inlineFieldToken.Text.Length;
            return true;
        }


        public bool TryLexWhitespace(string line, int lineIndex, ref int charIndex)
        {
            var match = Regex.Match(line, @"^(\s+)");
            if (!match.Success) return false;

            var spaceToken = new SpaceToken() {Line = lineIndex, Char = charIndex, Text = match.Groups[0].Value};
            Tokens.Add(spaceToken);
            charIndex += spaceToken.Text.Length;
            return true;
        }


        private static Regex NoteRegex =
            new Regex(@"^(?<acc>\^|\^\^|=|_|__)?(?<note>[a-gA-G])(?<oct>[,']*)(?<num>\d+)?(?<slash>/+)?(?<den>\d+)?");

        public bool TryLexNote(string line, int lineIndex, ref int charIndex)
        {
            var match = NoteRegex.Match(line);
            if (!match.Success) return false;

            var groups = match.Groups;
            var octave = char.IsLower(groups["note"].Value, 0) ? 1 : 0;
            if (groups["oct"].Success)
            {
                var unparsedOctave = groups["oct"].Value;
                octave -= unparsedOctave.Count(c => c == ',');
                octave += unparsedOctave.Count(c => c == '\'');
            }

            var num = groups["num"].Success ? int.Parse(groups["num"].Value) : 1;
            var denom = 0;
            if (groups["den"].Success)
                denom = int.Parse(groups["den"].Value);
            else if (groups["slash"].Success)
                denom = 2 * groups["slash"].Value.Count(c => c == '/');
            else
                denom = 1;

            var noteToken = new NoteToken()
            {
                Key = this.Key,
                TimeSignature = this.TimeSignature,
                Note = groups["note"].Value,
                Accidental = groups["acc"].Value,
                Octave = octave,
                Length = new Fraction(num, denom),
                Line = lineIndex,
                Char = charIndex,
                Text = match.Value
            };
            Tokens.Add(noteToken);

            if (HasPendingDots)
            {
                noteToken.Dotify(PendingDots, "right");
                PendingDots = null;
            }

            charIndex += match.Length;
            return true;
        }

        private static readonly string[] BeamChordSymbols = {"[", "]"};

        private bool TryLexBeam(string part, int lineIndex, ref int charIndex)
        {
            var match = Regex.Match(part, @"^([\[\]\|\:]+)([0-9\-,])?");
            if (!match.Success) return false;

            Token token;
            if (BeamChordSymbols.Contains(match.Value))
            {
                token = new ChordBracketToken() {Line = lineIndex, Char = charIndex, Text = match.Value};
            }
            else
            {
                token = new BeamToken() {Line = lineIndex, Char = charIndex, Text = match.Value};
            }

            charIndex += token.Text.Length;
            return true;
        }

        public IList<Token> Tokenize(IList<string> tune, Dictionary<string, string> tuneHeader,
            Dictionary<string, string> fileHeader)
        {
            Key = Key.FromName(fileHeader[InfoFields.Key.Name]);
            if (!fileHeader.TryGetValue(InfoFields.Meter.Name, out var meter))
                meter = "free";
            if (!fileHeader.TryGetValue(InfoFields.UnitNoteLength.Name, out var unit))
                if (meter != "free")
                {
                    throw new NotSupportedException("unit must be specified in free meter (for now)");
                    // //TODO convert meter fraction to decimal properly
                    // //if im correct, this implimentation should always fail
                    // if (float.TryParse(meter, out var meter_unit))
                    // {
                    //     if (meter_unit < 0.75f)
                    //     {
                    //         unit = "1/16";
                    //     }
                    //     else
                    //     {
                    //         unit = "1/8";
                    //     }
                    // }
                    // else throw new NotImplementedException();
                }

            fileHeader.TryGetValue(InfoFields.Tempo.Name, out var tempo);
            var meterFraction = TimeSignature.ParseMeter(meter);
            var unitFraction = TimeSignature.ParseFraction(unit);
            TimeSignature = new TimeSignature(meterFraction, unitFraction, tempo);
            for (var lineIndex = 0; lineIndex < tune.Count; lineIndex++)
            {
                var line = tune[lineIndex].TrimEnd();
                if (TryLexHeader(line, lineIndex))
                    continue;

                var charIndex = 0;
                while (charIndex < line.Length)
                {
                    var part = line.Substring(charIndex);
                    //Field
                    if (TryLexInlineField(part, lineIndex, ref charIndex))
                        continue; //Continue on success


                    //Space
                    if (TryLexWhitespace(part, lineIndex, ref charIndex))
                        continue;

                    //Notes
                    if (TryLexNote(part, lineIndex, ref charIndex))
                        continue;

                    //Beam
                    if (TryLexBeam(part, lineIndex, ref charIndex))
                        continue;


                    //BROKEN RHYTHM
                    if (TryLexBrokenRythem(part, lineIndex, ref charIndex))
                        continue;

                    //REST
                    if (TryLexRest(part, lineIndex, ref charIndex))
                        continue;

                    if (TryLexTuplets(part, lineIndex, ref charIndex))
                        continue;
                    if (TryLexSlur(part, lineIndex, ref charIndex))
                        continue;
                    if (TryLexTie(part, lineIndex, ref charIndex))
                        continue;
                    if (TryLexEmbelishments(part, lineIndex, ref charIndex))
                        continue;
                    if (TryLexDecorationsSingle(part, lineIndex, ref charIndex))
                        continue;
                    if (TryLexDecorationsMulti(part, lineIndex, ref charIndex))
                        continue;
                    // LINE NOT PART      V V V
                    if (TryLexContinuation(line, lineIndex, ref charIndex))
                        continue;
                    if (TryLexAnnotation(part, lineIndex, ref charIndex))
                        continue;
                    if (TryLexChord(part, lineIndex, ref charIndex))
                        continue;

                    throw new NotSupportedException(part);
                }

                if (Tokens[Tokens.Count - 1] is ContinuationToken)
                {
                    var token = new NewlineToken() {Line = lineIndex, Char = charIndex, Text = "\n"};
                    Tokens.Add(token);
                }

            }

            var cache = Tokens.ToArray();
            Clear();
            return cache;
        }

        private bool TryLexAnnotation(string part, int lineIndex, ref int charIndex)
        {
            var match = Regex.Match(part, @"^""[\^_\<\>\@][^""]+""");
            if (!match.Success) return false;
            var token = new AnnotationToken()
            {
                Char = charIndex,
                Line = lineIndex,
                Text = match.Value,
            };
            Tokens.Add(token);
            charIndex += token.Text.Length;
            return true;
        }

        private bool TryLexChord(string part, int lineIndex, ref int charIndex)
        {
            var match = Regex.Match(part, @"^""[\w#/]+""");
            if (!match.Success) return false;
            var token = new ChordSymbolToken()
            {
                Char = charIndex,
                Line = lineIndex,
                Text = match.Value,
            };
            Tokens.Add(token);
            charIndex += token.Text.Length;
            return true;
        }

        private bool TryLexContinuation(string line, int lineIndex, ref int charIndex)
        {
            if (line.Length - 1 != charIndex || line[charIndex] != '\\')
                return false;
            var token = new ContinuationToken()
            {
                Line = lineIndex,
                Char = charIndex,
                Text = "\\"
            };
            Tokens.Add(token);
            charIndex += token.Text.Length;
            return true;

        }

        private bool TryLexTie(string part, int lineIndex, ref int charIndex)
        {
            if (part[0] != '-') return false;
            var token = new TieToken()
            {
                Line = lineIndex,
                Char = charIndex,
                Text = part[0].ToString(),
            };
            Tokens.Add(token);
            charIndex += token.Text.Length;
            return true;
        }
        
        private bool TryLexSlur(string part, int lineIndex, ref int charIndex)
        {
            
            if (part[0] != '(' && part[0] != ')') return false;
            var token = new SlurToken()
            {
                Line = lineIndex,
                Char = charIndex,
                Text = part[0].ToString(),
            };
            Tokens.Add(token);
            charIndex += token.Text.Length;
            return true;
        }

        private bool TryLexTuplets(string part, int lineIndex, ref int charIndex)
        {
            var match = Regex.Match(part, @"^\(([2-9])");
            if (!match.Success) return false;
            var token = new TupletToken()
            {
                Line = lineIndex,
                Char = charIndex,
                Text = match.Value,
                Num = int.Parse(match.Groups[1].Value)
            };
            Tokens.Add(token);
            charIndex += token.Text.Length;
            return true;

        }

        private bool TryLexEmbelishments(string part, int lineIndex, ref int charIndex)
        {
            var match = Regex.Match(part, @"^(\{\\?)|\}");
            if (!match.Success) return false;
            var token = new GracenoteBraceToken()
            {
                Line = lineIndex,
                Char = charIndex,
                Text = match.Value,
            };
            Tokens.Add(token);
            charIndex += token.Text.Length;
            return true;
        }

        private bool TryLexDecorationsSingle(string part, int lineIndex, ref int charIndex)
        {
            var match = Regex.Match(part[0].ToString(), @"^[.~HLMOPSTuv]");
            if (!match.Success) return false;
            var token = new DecorationToken()
            {
                Line = lineIndex,
                Char = charIndex,
                Text = match.Value,
            };
            Tokens.Add(token);
            charIndex += token.Text.Length;
            return true;
        }

        private bool TryLexDecorationsMulti(string part, int lineIndex, ref int charIndex)
        {
            var match = Regex.Match(part[0].ToString(), @"^\!([^\! ]+)\!");
            if (!match.Success) return false;
            var token = new DecorationToken()
            {
                Line = lineIndex,
                Char = charIndex,
                Text = match.Value,
            };
            Tokens.Add(token);
            charIndex += token.Text.Length;
            return true;
        }

        private void Clear()
        {
            Tokens.Clear();
            //TODO clear rest
        }

        private static readonly Regex RestRegex = new Regex(@"^([XZxz])(\d+)?(/(\d+)?)?");

        private bool TryLexRest(string part, int lineIndex, ref int charIndex)
        {
            var match = RestRegex.Match(part);
            if (!match.Success) return false;
            
            var groups = match.Groups;
            var numeratorGroup = groups[2];
            var denominatorGroup = groups[4];
            var numerator = (numeratorGroup.Success) ? int.Parse(numeratorGroup.Value) : 1;
            var denominator = (denominatorGroup.Success) ? int.Parse(denominatorGroup.Value) : 1;

            if (groups[1].Value == "X" || groups[1].Value == "Z")
            {
                numerator = numerator * TimeSignature.Meter.Denominator;
                denominator = denominator * TimeSignature.Meter.Numerator;
            }


            var token = new RestToken()
            {
                Char = charIndex,
                Length = new Fraction(numerator, denominator),
                Line = lineIndex,
                Text = match.Value,
                Symbol = match.Value
            };
            Tokens.Add(token);
            if (HasPendingDots)
            {
                token.Dotify(PendingDots, "right");
                PendingDots = null;
            }

            charIndex += token.Text.Length;
            return true;

        }

        private bool TryLexBrokenRythem(string part, int lineIndex, ref int charIndex)
        {
            if (Tokens.Count <= 0) return false;
            var prevToken = Tokens[Tokens.Count - 1];
            var match = Regex.Match(part, @"^<+|>+");
            if (!match.Success) return false;

            switch (prevToken)
            {
                case NoteToken prevNote:
                    prevNote.Dotify(part, "left");
                    PendingDots = part;
                    charIndex += match.Length;
                    break;
                case RestToken prevRest:
                    prevRest.Dotify(part, "left");
                    PendingDots = part;
                    charIndex += match.Length;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return true;
        }
    }
}