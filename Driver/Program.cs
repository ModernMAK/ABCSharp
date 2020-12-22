using System;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using ABCSharp;
namespace Driver
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var wd = "";
            var song = "test.abc";

            var path = Path.Combine(wd, song);
            
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    Console.Out.WriteLine("Reading...");
                    var abc = reader.ReadToEnd();
                    Console.Out.WriteLine("Parsing...");
                    var parser = TuneParser.FromABC(abc);
                    Console.Out.WriteLine("Done");
                    Console.Out.WriteLine("Press Any Key To Continue...");
                    Console.In.ReadLine();
                }
            }
        }
    }
}