namespace Tx.ApplicationInsights.Session
{
    public class ParsedStack
    {
        public int Level { get; set; }
        public string Method { get; set; }
        public string Assembly { get; set; }
        public string FileName { get; set; }
        public int Line { get; set; }
    }
}