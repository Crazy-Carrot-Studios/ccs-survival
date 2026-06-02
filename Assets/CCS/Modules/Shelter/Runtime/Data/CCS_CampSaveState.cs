namespace CCS.Modules.Shelter
{
    public sealed class CCS_CampSaveState
    {
        public int campTier;
        public bool ownsCamp;
        public string campOwnerId = string.Empty;
        public float campCenterX;
        public float campCenterY;
        public float campCenterZ;
        public bool hasShelter;
        public bool hasCampfire;
        public bool hasBedroll;
    }
}
