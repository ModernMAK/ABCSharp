using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ABCSharp.Tokens;

namespace ABCSharp
{
    public class TuneParser
    {
        //Constructor is protected, use a FROM method to create
        private TuneLexer _lexer;

        protected TuneParser()
        {
            _lexer = new TuneLexer();
        }

        public static TuneParser FromABC(string abc)
        {
            var t = new TuneParser();
            t.ParseABC(abc);
            return t;
        }

        public string URL
        {
            get
            {
                if (!Header.TryGetValue(InfoFields.ReferenceNumber.Name, out var reference_number) ||
                    !Header.TryGetValue("setting", out var setting))
                {
                    return "";
                }

                return $"http://thesession.org/tunes/{reference_number}#setting{setting}";
            }
        }

        public IList<Token> Tokens { get; set; }
        
        public Dictionary<string, string> Header { get; set; }

        public string ABC { get; set; }

        public void ParseABC(string abc)
        {
            ABC = abc;
            var header = new List<string>();
            var tune = new List<string>();

            var parsing_tune = false;
            var lines = abc.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            foreach (var raw_line in lines)
            {
                var line = raw_line;
                // VALIDATE THIS WORKS
                // line = re.split(r'([^\\]|^)%', line)[0]
                var regex = new Regex(@"([^\\]|^)%");
                var line_split = regex.Split(line);
                line = line_split[0];
                line = line.Trim();

                if (line == "")
                    continue;
                if (parsing_tune)
                {
                    tune.Add(line);
                }
                else
                {
                    if (InfoFields.IsHeader(line, InfoFields.InfoKeys))
                    {
                        header.Add(line);
                        if (InfoFields.IsHeader(line, InfoFields.Key))
                        {
                            parsing_tune = true;
                        }
                    }
                    else if (InfoFields.IsHeaderContinuation(line))
                    {
                        header[header.Count - 1] += " " + InfoFields.StripHeader(line);
                    }
                }
            }

            ParseHeader(header);
            ParseTune(tune);
        }

        private void ParseTune(IList<string> tune)
        {
            Tokens = Tokenize(tune, Header);
        }


        public IList<Token> Tokenize(IList<string> tune, Dictionary<string, string> header)
        {
            return _lexer.Tokenize(tune, header, Header);
        }
        // var key = ABCSharp.Key.FromName(Header[InfoFields.Key.Name]);
            // if (!Header.TryGetValue(InfoFields.Meter.Name, out var meter))
            //     meter = "free";
            // if (!Header.TryGetValue(InfoFields.UnitNoteLength.Name, out var unit))
            //     if (meter != "free")
            //     {
            //         throw new NotImplementedException();
            //         //TODO convert meter fraction to decimal properly
            //         //if im correct, this implimentation should always fail
            //         if (float.TryParse(meter, out var meter_unit))
            //         {
            //             if (meter_unit < 0.75f)
            //             {
            //                 unit = "1/16";
            //             }
            //             else
            //             {
            //                 unit = "1/8";
            //             }
            //         }
            //         else throw new NotImplementedException();
            //     }
            //
            // Header.TryGetValue(InfoFields.Tempo.Name, out var tempo);
            // var meterFraction = TimeSignature.ParseMeter(meter);
            // var unitFraction = TimeSignature.ParseFraction(unit);
            // var time_sig = new TimeSignature(meterFraction, unitFraction, tempo);
            // var tokens = new List<Token>();
            // for (var i = 0; i < tune.Count; i++)
            // {
            //     var line = tune[i].TrimEnd();
            //     if (line.Length > 2 && (InfoFields.IsHeader(line, InfoFields.TuneBodyInfoKeys) ||
            //                             InfoFields.IsHeaderContinuation(line)))
            //     {
            //         var bodyFieldToken = new BodyFieldToken() {Line = i, Char = 0, Text = line};
            //         tokens.Add(bodyFieldToken);
            //         continue;
            //     }
            //
            //     bool has_pending_dots = false;
            //     string pending_dots = null;
            //     var j = 0;
            //     //TODO alot of regex
            //     while (j < line.Length)
            //     {
            //         var part = line.Substring(j);
            //         //Field
            //         if (part[0] == '[' && part.Length > 3 && part[2] == ':')
            //         {
            //             var inline_fields = new string(InfoFields.InlineInfoKeys.Keys.ToArray());
            //             var inline_regex = new Regex($@"\[[{inline_fields}]:([^\]]+)\]");
            //             var match = inline_regex.Match(part);
            //             if (match.Success)
            //             {
            //                 var match_line = match.Groups[1].Value;
            //                 //Original didnt check for "K:" just "K" so this may fail
            //                 //Since its not a 1-1 translation
            //                 if (InfoFields.IsHeader(match_line, InfoFields.Key))
            //                 {
            //                     key = ABCSharp.Key.FromName(match_line.Substring(3, line.Length - 3 - 1));
            //                 }
            //
            //                 var inlineFieldToken = new InlineFieldToken()
            //                     {Line = i, Char = j, Text = match_line};
            //                 j += match.Length;
            //                 continue;
            //             }
            //         }
            //
            //         //Space
            //         var whitespace_match = Regex.Match(part, @"(\s+)");
            //         if (whitespace_match.Success)
            //         {
            //             var spaceToken = new SpaceToken() {Line = i, Char = j, Text = whitespace_match.Groups[0].Value};
            //             tokens.Add(spaceToken);
            //             j += spaceToken.Text.Length;
            //         }
            //
            //         var note_match = Regex.Match(part,
            //             @"(?<acc>\^|\^\^|=|_|__)?(?<note>[a-gA-G])(?<oct>[,']*)(?<num>\d+)?(?<slash>/+)?(?<den>\d+)?");
            //
            //         if (note_match.Success)
            //         {
            //             var groups = note_match.Groups;
            //             var octave = char.IsLower(groups["note"].Value, 0) ? 1 : 0;
            //             if (groups["oct"].Success)
            //             {
            //                 var octave_str = groups["oct"].Value;
            //                 octave -= octave_str.Count(c => c == ',');
            //                 octave += octave_str.Count(c => c == '\'');
            //             }
            //
            //             var num = groups["num"].Success ? int.Parse(groups["num"].Value) : 1;
            //             var denom = 0;
            //             if (groups["den"].Success)
            //                 denom = int.Parse(groups["den"].Value);
            //             else if (groups["slash"].Success)
            //                 denom = 2 * groups["slash"].Value.Count(c => c == '/');
            //             else
            //                 denom = 1;
            //
            //             var noteToken = new NoteToken()
            //             {
            //                 Key = key,
            //                 TimeSignature = time_sig,
            //                 Note = groups["note"].Value,
            //                 Accidental = groups["acc"].Value,
            //                 Octave = octave,
            //                 Length = new Fraction(num, denom),
            //                 Line = i,
            //                 Char = j,
            //                 Text = note_match.Value
            //             };
            //             tokens.Add(noteToken);
            //
            //             if (has_pending_dots)
            //             {
            //                 noteToken.Dotify(pending_dots, "right");
            //                 pending_dots = null;
            //                 has_pending_dots = false;
            //             }
            //
            //             j += note_match.Length;
            //             continue;
            //         }
            //
            //         //Beam
            //         var beam_match = Regex.Match(part, @"([\[\]\|\:]+)([0-9\-,])?");
            //         if (beam_match.Success)
            //         {
            //             var chordSymbols = new[] {"[", "]"};
            //             if (chordSymbols.Contains(beam_match.Value))
            //             {
            //                 var chordToken = new ChordBracketToken() {Line = i, Char = j, Text = beam_match.Value};
            //                 tokens.Add(chordToken);
            //             }
            //             else
            //             {
            //                 var beamToken = new BeamToken() {Line = i, Char = j, Text = beam_match.Value};
            //                 tokens.Add(beamToken);
            //             }
            //
            //             j += beam_match.Value.Length;
            //             continue;
            //         }
            //
            //         //BROKEN RHYTHM
            //         var prevToken = tokens[tokens.Count - 1];
            //         if (tokens.Count > 0)
            //         {
            //             var broken_rythm_match = Regex.Match(part, @"<+|>+");
            //             if (broken_rythm_match.Success)
            //                 if (prevToken is NoteToken prevNote)
            //                 {
            //                     prevNote.Dotify(part, "left");
            //                     pending_dots = part;
            //                     has_pending_dots = true;
            //                     // j += br
            //                 }
            //                 else if (prevToken is RestToken prevRest)
            //                 {
            //                     //This seems to be a bug in pyabc; as dotify is only declared for Notes 
            //                     throw new NotSupportedException("Rest has no method dotify!");
            //                 }
            //         }
            //     }
            // }
            //
            // throw new NotImplementedException();
        // }
        ////TODO
        //// public void ParseJSON()

        public void ParseHeader(IEnumerable<string> header)
        {
            var formattedHeader = new Dictionary<string, string>();
            var infoKeys = InfoFields.InfoKeys;
            foreach (var line in header)
            {
                var header_key = line[0];
                var header_info = line.Substring(2);
                formattedHeader[infoKeys[header_key].Name] = header_info;
            }

            Header = formattedHeader;
            ReferenceNumber = formattedHeader[InfoFields.ReferenceNumber.Name];
            Title = formattedHeader[InfoFields.TuneTitle.Name];
            Key = formattedHeader[InfoFields.Key.Name];
        }

        public string Key { get; set; }
        public string Title { get; set; }
        public string ReferenceNumber { get; set; }
    }
}