using System;

namespace ABCSharp
{
    public struct Key
    {
        public Key(Pitch root, object mode)
        {
            Root = root;
            Mode = mode;
        }

        public static Key FromName(string name)
        {
            Console.Out.WriteLine("'Key.FromName(name)' Not Implimented, Ignoring");
            // throw new NotImplementedException();
            return new Key();
        }

        public Pitch Root { get; }
        public object Mode { get; }
    }
}