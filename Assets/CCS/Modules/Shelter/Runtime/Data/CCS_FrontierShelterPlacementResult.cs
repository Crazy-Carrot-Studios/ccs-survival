namespace CCS.Modules.Shelter
{
    public sealed class CCS_FrontierShelterPlacementResult
    {
        private CCS_FrontierShelterPlacementResult(
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

        public static CCS_FrontierShelterPlacementResult Failure(string message)
        {
            return new CCS_FrontierShelterPlacementResult(false, false, false, message);
        }

        public static CCS_FrontierShelterPlacementResult Preview(bool isValid, string message)
        {
            return new CCS_FrontierShelterPlacementResult(true, true, isValid, message);
        }

        public static CCS_FrontierShelterPlacementResult Placed(string message)
        {
            return new CCS_FrontierShelterPlacementResult(true, false, true, message);
        }
    }
}
