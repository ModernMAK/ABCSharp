using System;
using System.Collections.Generic;

namespace ABCSharp
{
    public static class InfoFields
    {
        //The python solution they have is brilliant; very pythonic
        //But i feel C# would like it better if we used const-likes
        //It allows us to avoid using magic numbers and 

        //Typing info; i dont think its used in the parser, but this IS a blind idiot code translation
        private const string StringType = "string";
        private const string InstructionType = "instruction";
        private const string AnyType = "-";

        public const char HeaderSeparator = ':';
        public const char ContinuationSymbol = '+';


        public static bool IsHeader(string line, InfoKey key) => key.Key == line[0];

        public static bool IsHeader(string line, IReadOnlyDictionary<char, InfoKey> dictionary)
        {
            return dictionary.ContainsKey(line[0]) && line[1] == HeaderSeparator;
        }

        public static bool IsHeaderContinuation(string line)
        {
            return line[0] == ContinuationSymbol && line[1] == HeaderSeparator;
        }

        public static string StripHeader(string line) => line.Substring(2);


        public static readonly InfoKey Area = new InfoKey('A', "area", true, true, false, false, StringType);
        public static readonly InfoKey Book = new InfoKey('B', "book", true, true, false, false, StringType);
        public static readonly InfoKey Composer = new InfoKey('C', "composer", true, true, false, false, StringType);

        public static readonly InfoKey Discography =
            new InfoKey('D', "discography", true, true, false, false, StringType);

        public static readonly InfoKey FileUrl = new InfoKey('F', "file url", true, true, false, false, StringType);
        public static readonly InfoKey Group = new InfoKey('G', "group", true, true, false, false, StringType);
        public static readonly InfoKey History = new InfoKey('H', "history", true, true, false, false, StringType);

        public static readonly InfoKey Instruction =
            new InfoKey('I', "instruction", true, true, true, true, InstructionType);

        public static readonly InfoKey Key = new InfoKey('K', "key", false, true, true, true, InstructionType);

        public static readonly InfoKey UnitNoteLength =
            new InfoKey('L', "unit note length", true, true, true, true, InstructionType);

        public static readonly InfoKey Meter = new InfoKey('M', "meter", true, true, true, true, InstructionType);
        public static readonly InfoKey Macro = new InfoKey('m', "macro", true, true, true, true, InstructionType);

        public static readonly InfoKey Notes = new InfoKey('N', "notes", true, true, true, true, StringType);
        public static readonly InfoKey Origin = new InfoKey('O', "origin", true, true, false, false, StringType);

        public static readonly InfoKey Parts = new InfoKey('P', "parts", false, true, true, true, InstructionType);
        public static readonly InfoKey Tempo = new InfoKey('Q', "tempo", false, true, true, true, InstructionType);

        public static readonly InfoKey Rhythm = new InfoKey('R', "rhythm", true, true, true, true, StringType);

        public static readonly InfoKey Remark = new InfoKey('r', "remark", true, true, false, false, AnyType);

        public static readonly InfoKey Source = new InfoKey('S', "source", true, true, false, false, StringType);

        public static readonly InfoKey SymbolLine =
            new InfoKey('s', "symbol line", false, false, true, false, InstructionType);

        public static readonly InfoKey TuneTitle = new InfoKey('T', "tune title", false, true, true, false, StringType);

        public static readonly InfoKey
            UserDefined = new InfoKey('U', "user defined", true, true, true, true, InstructionType);

        public static readonly InfoKey Voice = new InfoKey('V', "voice", false, true, true, true, InstructionType);

        public static readonly InfoKey WordsUpper = new InfoKey('W', "words", false, true, true, false, StringType);
        public static readonly InfoKey WordsLower = new InfoKey('w', "words", false, false, true, false, StringType);

        public static readonly InfoKey ReferenceNumber =
            new InfoKey('X', "reference number", false, true, false, false, InstructionType);

        public static readonly InfoKey Transcription =
            new InfoKey('Z', "transcription", true, true, false, false, StringType);

        public static IReadOnlyList<InfoKey> InfoKeyList = new[]
        {
            Area, Book, Composer, Discography, FileUrl, Group, History, Instruction, Key, UnitNoteLength, Meter, Macro,
            Notes, Origin, Parts, Tempo, Rhythm, Remark, Source, SymbolLine, TuneTitle, UserDefined, Voice, WordsUpper,
            WordsLower, ReferenceNumber, Transcription,
        };

        public static readonly IReadOnlyDictionary<char, InfoKey>
            InfoKeys = CreateInfoKeyDict(InfoKeyList, key => true);

        public static readonly IReadOnlyDictionary<char, InfoKey>
            TuneBodyInfoKeys = CreateInfoKeyDict(InfoKeyList, key => key.TuneBody);

        public static readonly IReadOnlyDictionary<char, InfoKey>
            TuneHeaderInfoKeys = CreateInfoKeyDict(InfoKeyList, key => key.TuneHeader);

        public static readonly IReadOnlyDictionary<char, InfoKey>
            FileHeaderInfoKeys = CreateInfoKeyDict(InfoKeyList, key => key.FileHeader);

        public static readonly IReadOnlyDictionary<char, InfoKey>
            InlineInfoKeys = CreateInfoKeyDict(InfoKeyList, key => key.Inline);

        private static Dictionary<char, InfoKey> CreateInfoKeyDict(IEnumerable<InfoKey> infoKeys,
            Func<InfoKey, bool> func)
        {
            var dictionary = new Dictionary<char, InfoKey>();
            foreach (var ik in infoKeys)
                if (func(ik))
                    dictionary[ik.Key] = ik;
            return dictionary;
        }
    }
}