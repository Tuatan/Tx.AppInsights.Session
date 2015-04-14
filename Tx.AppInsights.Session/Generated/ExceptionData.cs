namespace Tx.ApplicationInsights.Session
{
    using System.Collections.Generic;

    public class ExceptionData
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string Message { get; set; }
        public bool HasFullStack { get; set; }
        public List<ParsedStack> ParsedStack { get; set; }
    }
}