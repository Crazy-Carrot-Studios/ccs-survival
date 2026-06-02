using System;
using System.Collections.Generic;
using CCS.Core;
using CCS.Modules.Crafting;
using CCS.Modules.Inventory;
using CCS.Survival;
using UnityEngine;

namespace CCS.Modules.Industry
{
    public sealed class CCS_IndustryService : CCS_ISurvivalService, CCS_IUpdatable
    {
        private readonly Dictionary<string, CCS_IndustryWorkstation> workstationsByInstanceId =
            new Dictionary<string, CCS_IndustryWorkstation>(StringComparer.Ordinal);

        private readonly Dictionary<string, CCS_IndustryJob> activeJobsByWorkstationId =
            new Dictionary<string, CCS_IndustryJob>(StringComparer.Ordinal);

        private CCS_IndustryProfile activeProfile;
        private CCS_PlayerInventoryService inventoryService;
        private CCS_CraftingService craftingService;
        private Func<Vector3, float, string, bool> workstationRoleProximityQuery;
        private Vector3 subjectPosition;
        private bool hasSubjectPosition;
        private bool isInitialized;

        public bool IsInitialized => isInitialized;

        public CCS_IndustryProfile ActiveProfile => activeProfile;

        public void Initialize()
        {
            isInitialized = true;
        }

        public void InitializeFromProfile(CCS_IndustryProfile profile)
        {
            activeProfile = profile;
            isInitialized = profile != null;
        }

        public void BindInventoryService(CCS_PlayerInventoryService service)
        {
            inventoryService = service;
        }

        public void BindCraftingService(CCS_CraftingService service)
        {
            craftingService = service;
        }

        public void BindWorkstationRoleProximityQuery(Func<Vector3, float, string, bool> query)
        {
            workstationRoleProximityQuery = query;
        }

        public void SetSubjectPosition(Vector3 position)
        {
            subjectPosition = position;
            hasSubjectPosition = true;
        }

        public void Tick(float deltaTime)
        {
            if (activeJobsByWorkstationId.Count == 0)
            {
                return;
            }

            List<string> completedWorkstationIds = null;
            foreach (KeyValuePair<string, CCS_IndustryJob> entry in activeJobsByWorkstationId)
            {
                CCS_IndustryJob job = entry.Value;
                if (job == null || job.isComplete)
                {
                    continue;
                }

                job.elapsedSeconds += deltaTime;
                if (job.elapsedSeconds >= job.durationSeconds)
                {
                    CompleteJob(job);
                    completedWorkstationIds ??= new List<string>();
                    completedWorkstationIds.Add(entry.Key);
                }
            }

            if (completedWorkstationIds == null)
            {
                return;
            }

            for (int index = 0; index < completedWorkstationIds.Count; index++)
            {
                activeJobsByWorkstationId.Remove(completedWorkstationIds[index]);
            }
        }

        public void RegisterWorkstation(CCS_IndustryWorkstation workstation)
        {
            if (workstation == null || string.IsNullOrWhiteSpace(workstation.WorkstationInstanceId))
            {
                return;
            }

            workstationsByInstanceId[workstation.WorkstationInstanceId] = workstation;
        }

        public void UnregisterWorkstation(CCS_IndustryWorkstation workstation)
        {
            if (workstation == null || string.IsNullOrWhiteSpace(workstation.WorkstationInstanceId))
            {
                return;
            }

            workstationsByInstanceId.Remove(workstation.WorkstationInstanceId);
            activeJobsByWorkstationId.Remove(workstation.WorkstationInstanceId);
        }

        public bool HasWorkstationRoleInRadius(Vector3 origin, float radius, string workstationRoleId)
        {
            if (workstationRoleProximityQuery != null)
            {
                return workstationRoleProximityQuery(origin, radius, workstationRoleId);
            }

            foreach (KeyValuePair<string, CCS_IndustryWorkstation> entry in workstationsByInstanceId)
            {
                CCS_IndustryWorkstation workstation = entry.Value;
                if (workstation != null
                    && string.Equals(workstation.WorkstationRoleId, workstationRoleId, StringComparison.OrdinalIgnoreCase)
                    && Vector3.Distance(origin, workstation.WorldPosition) <= radius)
                {
                    return true;
                }
            }

            return false;
        }

        public CCS_IndustryJobResult TryStartProcess(string processId)
        {
            if (!EnsureReady())
            {
                return CCS_IndustryJobResult.Failure("Industry service is not ready.");
            }

            if (!activeProfile.TryGetProcessById(processId, out CCS_IndustryDefinition definition))
            {
                return CCS_IndustryJobResult.Failure("Unknown industry process.");
            }

            if (definition.IsFuturePlaceholder)
            {
                return CCS_IndustryJobResult.Failure("Industry process is not available yet.");
            }

            if (!TryFindWorkstationForRole(definition.RequiredWorkstationRoleId, out CCS_IndustryWorkstation workstation))
            {
                return CCS_IndustryJobResult.Failure("Required industry workstation is not in camp range.");
            }

            if (activeJobsByWorkstationId.ContainsKey(workstation.WorkstationInstanceId))
            {
                return CCS_IndustryJobResult.Failure("Workstation is already processing.");
            }

            CCS_SurvivalValidationResult validation = CCS_IndustryValidationUtility.ValidateProcessDefinition(definition);
            if (!validation.IsSuccess)
            {
                return CCS_IndustryJobResult.Failure(validation.Message);
            }

            if (!HasInventoryForProcess(definition))
            {
                return CCS_IndustryJobResult.Failure("Missing ingredients for industry process.");
            }

            if (!ConsumeInputs(definition))
            {
                return CCS_IndustryJobResult.Failure("Failed to consume industry ingredients.");
            }

            if (definition.ProcessTimeSeconds <= 0f)
            {
                GrantOutputs(definition);
                return CCS_IndustryJobResult.Succeeded(definition, $"{definition.DisplayName} complete.");
            }

            CCS_IndustryJob job = new CCS_IndustryJob
            {
                jobId = Guid.NewGuid().ToString("N"),
                processId = definition.ProcessId,
                workstationInstanceId = workstation.WorkstationInstanceId,
                elapsedSeconds = 0f,
                durationSeconds = definition.ProcessTimeSeconds,
                isComplete = false
            };
            activeJobsByWorkstationId[workstation.WorkstationInstanceId] = job;
            return CCS_IndustryJobResult.Succeeded(definition, $"{definition.DisplayName} started.");
        }

        public CCS_IndustryJobResult TryCraftBlacksmithRecipe(string blacksmithRecipeId, CCS_CraftingStationContext stationContext)
        {
            if (!EnsureReady() || craftingService == null || !craftingService.IsInitialized)
            {
                return CCS_IndustryJobResult.Failure("Crafting service is not ready for forge work.");
            }

            CCS_BlacksmithRecipeDefinition blacksmithRecipe = FindBlacksmithRecipe(blacksmithRecipeId);
            if (blacksmithRecipe?.CraftingRecipe == null)
            {
                return CCS_IndustryJobResult.Failure("Unknown blacksmith recipe.");
            }

            if (blacksmithRecipe.Category == CCS_BlacksmithRecipeCategory.FutureWeapon)
            {
                return CCS_IndustryJobResult.Failure("Future firearm products are not available in this milestone.");
            }

            if (!HasWorkstationRoleInRadius(
                    hasSubjectPosition ? subjectPosition : Vector3.zero,
                    12f,
                    CCS_IndustryWorkstationRole.PrimitiveForge))
            {
                return CCS_IndustryJobResult.Failure("Primitive forge is required.");
            }

            CCS_CraftingResult craftResult = craftingService.TryCraft(
                new CCS_CraftingRequest(blacksmithRecipe.CraftingRecipe, stationContext, 1));

            return craftResult.IsSuccess
                ? CCS_IndustryJobResult.Succeeded(null, craftResult.Message)
                : CCS_IndustryJobResult.Failure(craftResult.Message);
        }

        public CCS_IndustryJob[] CaptureActiveJobs()
        {
            if (activeJobsByWorkstationId.Count == 0)
            {
                return Array.Empty<CCS_IndustryJob>();
            }

            CCS_IndustryJob[] jobs = new CCS_IndustryJob[activeJobsByWorkstationId.Count];
            int index = 0;
            foreach (KeyValuePair<string, CCS_IndustryJob> entry in activeJobsByWorkstationId)
            {
                jobs[index++] = entry.Value;
            }

            return jobs;
        }

        public void RestoreActiveJobs(CCS_IndustryJob[] jobs)
        {
            activeJobsByWorkstationId.Clear();
            if (jobs == null || jobs.Length == 0)
            {
                return;
            }

            for (int index = 0; index < jobs.Length; index++)
            {
                CCS_IndustryJob job = jobs[index];
                if (job == null || string.IsNullOrWhiteSpace(job.workstationInstanceId))
                {
                    continue;
                }

                if (job.isComplete || job.elapsedSeconds >= job.durationSeconds)
                {
                    if (activeProfile != null
                        && activeProfile.TryGetProcessById(job.processId, out CCS_IndustryDefinition definition))
                    {
                        GrantOutputs(definition);
                    }

                    continue;
                }

                activeJobsByWorkstationId[job.workstationInstanceId] = job;
            }
        }

        public void RestoreWorkstationJob(string workstationInstanceId, string activeProcessId, float elapsedSeconds)
        {
            if (string.IsNullOrWhiteSpace(workstationInstanceId)
                || string.IsNullOrWhiteSpace(activeProcessId)
                || activeProfile == null
                || !activeProfile.TryGetProcessById(activeProcessId, out CCS_IndustryDefinition definition))
            {
                return;
            }

            activeJobsByWorkstationId[workstationInstanceId] = new CCS_IndustryJob
            {
                jobId = Guid.NewGuid().ToString("N"),
                processId = activeProcessId,
                workstationInstanceId = workstationInstanceId,
                elapsedSeconds = elapsedSeconds,
                durationSeconds = definition.ProcessTimeSeconds,
                isComplete = false
            };
        }

        public CCS_IndustryJob CaptureWorkstationJob(string workstationInstanceId)
        {
            return activeJobsByWorkstationId.TryGetValue(workstationInstanceId, out CCS_IndustryJob job)
                ? job
                : null;
        }

        private void CompleteJob(CCS_IndustryJob job)
        {
            if (job == null || activeProfile == null)
            {
                return;
            }

            if (!activeProfile.TryGetProcessById(job.processId, out CCS_IndustryDefinition definition))
            {
                job.isComplete = true;
                return;
            }

            GrantOutputs(definition);
            job.isComplete = true;
        }

        private bool TryFindWorkstationForRole(string roleId, out CCS_IndustryWorkstation workstation)
        {
            workstation = null;
            float radius = 12f;
            Vector3 origin = hasSubjectPosition ? subjectPosition : Vector3.zero;
            float bestDistance = float.MaxValue;

            foreach (KeyValuePair<string, CCS_IndustryWorkstation> entry in workstationsByInstanceId)
            {
                CCS_IndustryWorkstation candidate = entry.Value;
                if (candidate == null
                    || !string.Equals(candidate.WorkstationRoleId, roleId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                float distance = Vector3.Distance(origin, candidate.WorldPosition);
                if (distance > radius)
                {
                    continue;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    workstation = candidate;
                }
            }

            return workstation != null;
        }

        private bool HasInventoryForProcess(CCS_IndustryDefinition definition)
        {
            for (int index = 0; index < definition.Inputs.Count; index++)
            {
                CCS_IndustryProcessStack stack = definition.Inputs[index];
                if (stack?.ItemDefinition == null)
                {
                    return false;
                }

                if (!inventoryService.HasItem(stack.ItemDefinition, stack.Quantity))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ConsumeInputs(CCS_IndustryDefinition definition)
        {
            for (int index = 0; index < definition.Inputs.Count; index++)
            {
                CCS_IndustryProcessStack stack = definition.Inputs[index];
                if (inventoryService.RemoveItem(stack.ItemDefinition, stack.Quantity) < stack.Quantity)
                {
                    return false;
                }
            }

            return true;
        }

        private void GrantOutputs(CCS_IndustryDefinition definition)
        {
            for (int index = 0; index < definition.Outputs.Count; index++)
            {
                CCS_IndustryProcessStack stack = definition.Outputs[index];
                if (stack?.ItemDefinition == null)
                {
                    continue;
                }

                inventoryService.AddItem(stack.ItemDefinition, stack.Quantity);
            }
        }

        private CCS_BlacksmithRecipeDefinition FindBlacksmithRecipe(string blacksmithRecipeId)
        {
            if (activeProfile?.BlacksmithRecipes == null || string.IsNullOrWhiteSpace(blacksmithRecipeId))
            {
                return null;
            }

            for (int index = 0; index < activeProfile.BlacksmithRecipes.Count; index++)
            {
                CCS_BlacksmithRecipeDefinition candidate = activeProfile.BlacksmithRecipes[index];
                if (candidate != null
                    && string.Equals(candidate.BlacksmithRecipeId, blacksmithRecipeId, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return null;
        }

        private bool EnsureReady()
        {
            return isInitialized
                && activeProfile != null
                && inventoryService != null
                && inventoryService.IsInitialized;
        }
    }
}
