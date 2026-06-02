namespace CCS.Modules.Industry
{
    public readonly struct CCS_IndustryJobResult
    {
        public CCS_IndustryJobResult(bool success, string message, CCS_IndustryDefinition processDefinition)
        {
            Success = success;
            Message = message ?? string.Empty;
            ProcessDefinition = processDefinition;
        }

        public bool Success { get; }

        public string Message { get; }

        public CCS_IndustryDefinition ProcessDefinition { get; }

        public static CCS_IndustryJobResult Failure(string message)
        {
            return new CCS_IndustryJobResult(false, message, null);
        }

        public static CCS_IndustryJobResult Succeeded(CCS_IndustryDefinition definition, string message)
        {
            return new CCS_IndustryJobResult(true, message, definition);
        }
    }
}
