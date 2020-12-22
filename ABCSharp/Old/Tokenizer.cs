// using System;
// using System.Collections.Generic;
//
// namespace ABCSharp
// {
//     public class Tokenizer : IDisposable
//     {
//         private static readonly string[] StringHeaderFields = new [] {
//             //Strings
//             "A", "B", "C", "D", "F", "G", "H",
//             "N", "O", "R",
//             "S",
//             "T",
//             "W", "w",
//             "Z",
//         };
//         private static readonly string[] InstructionHeaderFields = new [] {
//             //Instructions
//             "I", "K", "L", "M", "m",
//             "P", "Q",
//             "s",
//             "U", "V",
//             "X",
//         };
//
//         private static readonly string[] UntypedHeaderFields = new[]
//         {
//             //Any
//             "r",
//         };
//
//         private const string HeaderMarker = ":";
//
//         private static readonly string[][] AllHeaders = new[]
//         {
//             StringHeaderFields,
//             InstructionHeaderFields,
//             UntypedHeaderFields
//         };
//         
//         public Tokenizer()
//         {
//         }
//
//         private bool IsHeader(string line)
//         {
//             throw new NotImplementedException();
//             // if(line[1] != ":")
//             
//             foreach (var l in AllHeaders)
//             foreach(var h in l)
//             {
//                 // if(line[0] == h)
//             }
//             
//         }
//
//         public IEnumerable<KeyValuePair<TokenCode, string>> TokenizeLine(string line)
//         {
//             throw new NotImplementedException();
//             // if 
//         }
//
//         
//         
//         public void Dispose()
//         {
//         }
//     }
// }