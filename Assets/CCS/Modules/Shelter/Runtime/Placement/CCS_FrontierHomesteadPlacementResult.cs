namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierHomesteadPlacementResult
    {
        private CCS_FrontierHomesteadPlacementResult(
            bool isSuccess,
            bool isPreview,
            bool isValid,
            string message)
        {
            IsSuccess = isSuccess;
            IsPreview = isPreview;
            IsValid = isValid;
            Message = message ?? string.Empty;
        }

        public bool IsSuccess { get; }

        public bool IsPreview { get; }

        public bool IsValid { get; }

        public string Message { get; }

        public static CCS_FrontierHomesteadPlacementResult Failure(string message)
        {
            return new CCS_FrontierHomesteadPlacementResult(false, false, false, message);
        }

        public static CCS_FrontierHomesteadPlacementResult Preview(bool isValid, string message)
        {
            return new CCS_FrontierHomesteadPlacementResult(true, true, isValid, message);
        }

        public static CCS_FrontierHomesteadPlacementResult Placed(string message)
        {
            return new CCS_FrontierHomesteadPlacementResult(true, false, true, message);
        }
    }
}
