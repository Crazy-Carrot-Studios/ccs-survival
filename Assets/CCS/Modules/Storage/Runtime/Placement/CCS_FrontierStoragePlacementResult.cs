namespace CCS.Modules.Storage
{
    public sealed class CCS_FrontierStoragePlacementResult
    {
        private CCS_FrontierStoragePlacementResult(bool isSuccess, bool isPreview, bool isValid, string message)
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

        public static CCS_FrontierStoragePlacementResult Failure(string message)
        {
            return new CCS_FrontierStoragePlacementResult(false, false, false, message);
        }

        public static CCS_FrontierStoragePlacementResult Preview(bool isValid, string message)
        {
            return new CCS_FrontierStoragePlacementResult(true, true, isValid, message);
        }

        public static CCS_FrontierStoragePlacementResult Placed(string message)
        {
            return new CCS_FrontierStoragePlacementResult(true, false, true, message);
        }
    }
}
