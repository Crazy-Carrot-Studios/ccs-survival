using System;

namespace CCS.Modules.Shelter
{
    public sealed class CCS_CampSaveState
    {
        public int campTier;
        public bool ownsCamp;
        public string campOwnerId = string.Empty;
        public long campCreationTimeUtcTicks;
        public float campCenterX;
        public float campCenterY;
        public float campCenterZ;
        public bool hasShelter;
        public bool hasCampfire;
        public bool hasBedroll;
        public bool hasStorage;
        public bool hasWorkArea;
        public bool hasSawTable;
        public bool hasCharcoalKiln;
        public bool hasPrimitiveForge;
        public string landClaimId = string.Empty;
        public string[] structuresPresent = Array.Empty<string>();
    }
}
