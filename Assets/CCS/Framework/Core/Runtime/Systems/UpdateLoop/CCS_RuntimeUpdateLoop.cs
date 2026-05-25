using System.Collections.Generic;

// =============================================================================
// SCRIPT: CCS_RuntimeUpdateLoop
// CATEGORY: Core / Runtime / Systems
// PURPOSE: Coordinates Tick, FixedTick, and LateTick for registered CCS systems.
// PLACEMENT: Instantiated by bootstrap code. Not attached to GameObjects.
// AUTHOR: James Schilz
// CREATED: 2026-05-24
// NOTES: No MonoBehaviour bridge yet. No static singleton. No UnityEditor references.
// =============================================================================

namespace CCS.Core
{
    public sealed class CCS_RuntimeUpdateLoop
    {
        private const string LogCategory = "Runtime Update Loop";

        #region Variables

        private readonly List<CCS_IUpdatable> updatableSystems;
        private readonly List<CCS_IFixedUpdatable> fixedUpdatableSystems;
        private readonly List<CCS_ILateUpdatable> lateUpdatableSystems;
        private readonly bool enableDebugLogs;

        #endregion

        #region Public Methods

        public CCS_RuntimeUpdateLoop()
            : this(false)
        {
        }

        public CCS_RuntimeUpdateLoop(bool enableDebugLogs)
        {
            updatableSystems = new List<CCS_IUpdatable>();
            fixedUpdatableSystems = new List<CCS_IFixedUpdatable>();
            lateUpdatableSystems = new List<CCS_ILateUpdatable>();
            this.enableDebugLogs = enableDebugLogs;
        }

        public bool RegisterUpdatable(CCS_IUpdatable updatableSystem)
        {
            if (!CCS_Validation.IsObjectValid(updatableSystem))
            {
                return false;
            }

            if (updatableSystems.Contains(updatableSystem))
            {
                return false;
            }

            updatableSystems.Add(updatableSystem);
            CCS_Logger.Log(LogCategory, "Registered updatable system.", enableDebugLogs);
            return true;
        }

        public bool UnregisterUpdatable(CCS_IUpdatable updatableSystem)
        {
            if (!updatableSystems.Remove(updatableSystem))
            {
                return false;
            }

            CCS_Logger.Log(LogCategory, "Unregistered updatable system.", enableDebugLogs);
            return true;
        }

        public bool RegisterFixedUpdatable(CCS_IFixedUpdatable fixedUpdatableSystem)
        {
            if (!CCS_Validation.IsObjectValid(fixedUpdatableSystem))
            {
                return false;
            }

            if (fixedUpdatableSystems.Contains(fixedUpdatableSystem))
            {
                return false;
            }

            fixedUpdatableSystems.Add(fixedUpdatableSystem);
            CCS_Logger.Log(LogCategory, "Registered fixed updatable system.", enableDebugLogs);
            return true;
        }

        public bool UnregisterFixedUpdatable(CCS_IFixedUpdatable fixedUpdatableSystem)
        {
            if (!fixedUpdatableSystems.Remove(fixedUpdatableSystem))
            {
                return false;
            }

            CCS_Logger.Log(LogCategory, "Unregistered fixed updatable system.", enableDebugLogs);
            return true;
        }

        public bool RegisterLateUpdatable(CCS_ILateUpdatable lateUpdatableSystem)
        {
            if (!CCS_Validation.IsObjectValid(lateUpdatableSystem))
            {
                return false;
            }

            if (lateUpdatableSystems.Contains(lateUpdatableSystem))
            {
                return false;
            }

            lateUpdatableSystems.Add(lateUpdatableSystem);
            CCS_Logger.Log(LogCategory, "Registered late updatable system.", enableDebugLogs);
            return true;
        }

        public bool UnregisterLateUpdatable(CCS_ILateUpdatable lateUpdatableSystem)
        {
            if (!lateUpdatableSystems.Remove(lateUpdatableSystem))
            {
                return false;
            }

            CCS_Logger.Log(LogCategory, "Unregistered late updatable system.", enableDebugLogs);
            return true;
        }

        public void Tick(float deltaTime)
        {
            for (int index = 0; index < updatableSystems.Count; index++)
            {
                updatableSystems[index].Tick(deltaTime);
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
            for (int index = 0; index < fixedUpdatableSystems.Count; index++)
            {
                fixedUpdatableSystems[index].FixedTick(fixedDeltaTime);
            }
        }

        public void LateTick(float deltaTime)
        {
            for (int index = 0; index < lateUpdatableSystems.Count; index++)
            {
                lateUpdatableSystems[index].LateTick(deltaTime);
            }
        }

        public void Clear()
        {
            updatableSystems.Clear();
            fixedUpdatableSystems.Clear();
            lateUpdatableSystems.Clear();
            CCS_Logger.Log(LogCategory, "Cleared all registered update systems.", enableDebugLogs);
        }

        public CCS_UpdateLoopDiagnosticsInfo BuildDiagnosticsSnapshot()
        {
            return new CCS_UpdateLoopDiagnosticsInfo(
                updatableSystems.Count,
                fixedUpdatableSystems.Count,
                lateUpdatableSystems.Count);
        }

        #endregion
    }
}
