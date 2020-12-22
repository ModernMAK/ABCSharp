namespace ABCSharp
{
    public struct InfoKey
    {
        public InfoKey(char key, string name, bool fileHeader, bool tuneHeader, bool tuneBody, bool inline,
            string type)
        {
            Key = key;
            Name = name;
            Type = type;
            
            FileHeader = fileHeader;
            TuneHeader = tuneHeader;
            TuneBody = tuneBody;
            Inline = inline;
        }
        
        public char Key { get; }
        
        public string Name { get; }
        public bool FileHeader { get; }
        public bool TuneHeader { get; }
        public bool TuneBody { get; }
        public bool Inline { get; }
        public string Type { get; }
    }
}