namespace LAV.GraphDbFramework.Core.Exceptions
{
    public class GraphDbError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? Property { get; set; }
        public object? AttemptedValue { get; set; }
        public GraphDbError() { }

        public GraphDbError(string code, string message, string? property = null, object? attemptedValue = null)
        {
            Code = code;
            Message = message;
            Property = property;
            AttemptedValue = attemptedValue;
        }
    }
}