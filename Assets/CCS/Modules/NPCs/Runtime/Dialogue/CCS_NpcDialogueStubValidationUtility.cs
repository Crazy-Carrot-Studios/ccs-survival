using System;
using System.Collections.Generic;
using CCS.Modules.Settlements;
using CCS.Survival;

// =============================================================================
// SCRIPT: CCS_NpcDialogueStubValidationUtility
// CATEGORY: Modules / NPCs / Runtime / Dialogue
// PURPOSE: Profile validation and stub line matching for dialogue resolution.
// PLACEMENT: Used by CCS_NpcDialogueStubService, validators, and playtest harness.
// AUTHOR: James Schilz
// CREATED: 2026-05-28
// NOTES: Milestone 4.9.0 — most specific filter match wins per stub category.
// =============================================================================

namespace CCS.Modules.NPCs
{
    public static class CCS_NpcDialogueStubValidationUtility
    {
        private static readonly CCS_NpcRoleType[] ActiveRoleCoverage =
        {
            CCS_NpcRoleType.Merchant,
            CCS_NpcRoleType.Banker,
            CCS_NpcRoleType.Miner,
            CCS_NpcRoleType.Farmer,
            CCS_NpcRoleType.LumberWorker,
            CCS_NpcRoleType.Laborer,
            CCS_NpcRoleType.StableHand,
            CCS_NpcRoleType.Gunsmith,
            CCS_NpcRoleType.Blacksmith,
            CCS_NpcRoleType.Clerk
        };

        public static CCS_SurvivalValidationResult ValidateProfile(CCS_NpcDialogueStubProfile profile)
        {
            if (profile == null)
            {
                return CCS_SurvivalValidationResult.Fail("NPC dialogue stub profile is missing.");
            }

            if (string.IsNullOrWhiteSpace(profile.ProfileId))
            {
                return CCS_SurvivalValidationResult.Fail("NPC dialogue stub profile requires profileId.");
            }

            if (string.IsNullOrWhiteSpace(profile.GenericFallbackLine))
            {
                return CCS_SurvivalValidationResult.Fail("NPC dialogue stub profile requires generic fallback line.");
            }

            for (int index = 0; index < ActiveRoleCoverage.Length; index++)
            {
                CCS_NpcRoleType roleType = ActiveRoleCoverage[index];
                if (!profile.TryGetDefinitionForRole(roleType, out CCS_NpcDialogueStubDefinition definition)
                    || definition == null
                    || !DefinitionHasCategory(definition, CCS_NpcDialogueStubCategory.RoleIntroduction))
                {
                    return CCS_SurvivalValidationResult.Fail(
                        $"NPC dialogue stub profile missing role introduction for {roleType}.");
                }
            }

            if (!GlobalLinesContainCategory(profile.GlobalLines, CCS_NpcDialogueStubCategory.Greeting))
            {
                return CCS_SurvivalValidationResult.Fail("NPC dialogue stub profile missing global greeting line.");
            }

            if (!GlobalLinesContainCategory(profile.GlobalLines, CCS_NpcDialogueStubCategory.ServiceHint))
            {
                return CCS_SurvivalValidationResult.Fail(
                    "NPC dialogue stub profile missing service representative hint lines.");
            }

            return CCS_SurvivalValidationResult.Pass(
                $"NPC dialogue stub profile validated ({profile.ProfileId}).");
        }

        public static CCS_NpcDialogueStubResult ResolveDialogue(
            CCS_NpcDialogueStubProfile profile,
            CCS_NpcDialogueStubRequest request)
        {
            if (request == null)
            {
                return Fail(CCS_NpcDialogueStubResultType.InvalidTarget, "Dialogue request is null.");
            }

            if (profile == null)
            {
                return Fail(CCS_NpcDialogueStubResultType.Failed, "Dialogue stub profile unavailable.");
            }

            if (!request.HasIdentity)
            {
                return Fail(CCS_NpcDialogueStubResultType.NoIdentity, "NPC identity missing for dialogue stub.");
            }

            if (profile.RequireAffiliationForDialogue && !request.HasSettlement)
            {
                return Fail(CCS_NpcDialogueStubResultType.NoAffiliation, "NPC settlement affiliation missing.");
            }

            if (!request.HasRole)
            {
                return Fail(CCS_NpcDialogueStubResultType.NoRole, "NPC role missing for dialogue stub.");
            }

            List<CCS_NpcDialogueStubLine> candidates = CollectCandidateLines(profile, request);
            string greeting = ResolveBestLine(candidates, request, CCS_NpcDialogueStubCategory.Greeting)
                ?? profile.GenericFallbackLine;
            string roleIntro = ResolveBestLine(candidates, request, CCS_NpcDialogueStubCategory.RoleIntroduction);
            string settlementIntro =
                ResolveBestLine(candidates, request, CCS_NpcDialogueStubCategory.SettlementIntroduction);
            string businessIntro =
                ResolveBestLine(candidates, request, CCS_NpcDialogueStubCategory.BusinessIntroduction);
            string serviceHint = request.IsServiceRepresentative
                ? ResolveBestLine(candidates, request, CCS_NpcDialogueStubCategory.ServiceHint)
                : string.Empty;
            string fallback = ResolveBestLine(candidates, request, CCS_NpcDialogueStubCategory.GenericFallback)
                ?? profile.GenericFallbackLine;

            if (string.IsNullOrWhiteSpace(roleIntro) && string.IsNullOrWhiteSpace(greeting))
            {
                return Fail(CCS_NpcDialogueStubResultType.NoMatchingStub, "No matching dialogue stub lines found.");
            }

            List<string> displayLines = new List<string>();
            AppendLine(displayLines, greeting);
            AppendLine(displayLines, roleIntro);
            AppendLine(displayLines, settlementIntro);
            AppendLine(displayLines, businessIntro);
            AppendLine(displayLines, serviceHint);
            string eventLine = CCS_SettlementEventRuntimeBridge.ResolveDialogueAppendLine != null
                ? CCS_SettlementEventRuntimeBridge.ResolveDialogueAppendLine.Invoke(request.SettlementId)
                : string.Empty;
            AppendLine(displayLines, eventLine);
            string rumorLine = CCS_SettlementNewsRuntimeBridge.ResolveRumorDialogueAppendLine != null
                ? CCS_SettlementNewsRuntimeBridge.ResolveRumorDialogueAppendLine.Invoke(request.SettlementId)
                : string.Empty;
            AppendLine(displayLines, rumorLine);
            if (displayLines.Count == 0)
            {
                AppendLine(displayLines, fallback);
            }

            return new CCS_NpcDialogueStubResult
            {
                ResultType = CCS_NpcDialogueStubResultType.Success,
                Message = "Dialogue stub resolved.",
                NpcIdentityId = request.NpcIdentityId,
                DisplayName = request.DisplayName,
                RoleDisplayName = request.RoleDisplayName,
                SettlementDisplayName = string.IsNullOrWhiteSpace(request.SettlementDisplayName)
                    ? request.SettlementId
                    : request.SettlementDisplayName,
                BusinessDisplayName = string.IsNullOrWhiteSpace(request.BusinessDisplayName)
                    ? request.BusinessId
                    : request.BusinessDisplayName,
                GreetingLine = greeting ?? string.Empty,
                RoleIntroductionLine = roleIntro ?? string.Empty,
                SettlementIntroductionLine = settlementIntro ?? string.Empty,
                BusinessIntroductionLine = businessIntro ?? string.Empty,
                ServiceHintLine = serviceHint ?? string.Empty,
                GenericFallbackLine = fallback ?? string.Empty,
                DisplayLines = displayLines.ToArray()
            };
        }

        public static CCS_NpcDialogueStubRequest BuildRequestFromHost(CCS_INpcMovementHost host)
        {
            CCS_NpcDialogueStubRequest request = new CCS_NpcDialogueStubRequest();
            if (host == null || !host.HasIdentity)
            {
                return request;
            }

            request.NpcIdentityId = host.NpcIdentityId;
            request.SettlementId = host.SettlementId;
            request.BusinessId = host.BusinessId;
            request.IsServiceRepresentative = host.IsServiceRepresentative;

            if (host is CCS_IPopulationPlaceholderIdentityHost identityHost)
            {
                request.DisplayName = identityHost.DisplayName;
                request.RoleDisplayName = identityHost.IsServiceRepresentative
                    && !string.IsNullOrWhiteSpace(identityHost.RepresentativeTitle)
                    ? identityHost.RepresentativeTitle
                    : identityHost.RoleDisplayName;
                request.RoleType = Enum.IsDefined(typeof(CCS_NpcRoleType), identityHost.RoleType)
                    ? (CCS_NpcRoleType)identityHost.RoleType
                    : CCS_NpcRoleType.Unknown;
            }

            if (CCS_NpcAffiliationRuntimeBridge.TryGetAffiliationSnapshot(
                    host.SettlementId,
                    host.NpcIdentityId,
                    out CCS_NpcAffiliationSnapshot affiliationSnapshot)
                && affiliationSnapshot != null
                && affiliationSnapshot.IsValid)
            {
                request.SettlementDisplayName = affiliationSnapshot.SettlementDisplayName;
                request.BusinessDisplayName = affiliationSnapshot.BusinessDisplayName;
                request.RegionId = affiliationSnapshot.RegionId;
                request.PrimaryAffiliationType = affiliationSnapshot.IsServiceRepresentative
                    ? CCS_NpcAffiliationType.Business
                    : CCS_NpcAffiliationType.Workforce;
            }
            else if (!string.IsNullOrWhiteSpace(host.SettlementId))
            {
                request.PrimaryAffiliationType = CCS_NpcAffiliationType.Settlement;
            }

            return request;
        }

        private static List<CCS_NpcDialogueStubLine> CollectCandidateLines(
            CCS_NpcDialogueStubProfile profile,
            CCS_NpcDialogueStubRequest request)
        {
            List<CCS_NpcDialogueStubLine> candidates = new List<CCS_NpcDialogueStubLine>();
            if (profile.TryGetDefinitionForRole(request.RoleType, out CCS_NpcDialogueStubDefinition definition)
                && definition != null)
            {
                candidates.AddRange(definition.Lines);
            }

            candidates.AddRange(profile.GlobalLines);
            return candidates;
        }

        private static string ResolveBestLine(
            List<CCS_NpcDialogueStubLine> candidates,
            CCS_NpcDialogueStubRequest request,
            CCS_NpcDialogueStubCategory category)
        {
            CCS_NpcDialogueStubLine bestLine = null;
            int bestScore = int.MinValue;
            for (int index = 0; index < candidates.Count; index++)
            {
                CCS_NpcDialogueStubLine line = candidates[index];
                if (line == null || line.Category != category)
                {
                    continue;
                }

                int score = ScoreLineMatch(line, request);
                if (score < 0)
                {
                    continue;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestLine = line;
                }
            }

            return bestLine?.LineText;
        }

        private static int ScoreLineMatch(CCS_NpcDialogueStubLine line, CCS_NpcDialogueStubRequest request)
        {
            int score = 0;
            if (line.RoleType != CCS_NpcRoleType.Unknown
                && line.RoleType != request.RoleType)
            {
                return -1;
            }

            if (line.RoleType == request.RoleType)
            {
                score += 8;
            }

            if (!string.IsNullOrWhiteSpace(line.SettlementId))
            {
                if (!string.Equals(line.SettlementId, request.SettlementId, StringComparison.OrdinalIgnoreCase))
                {
                    return -1;
                }

                score += 4;
            }

            if (!string.IsNullOrWhiteSpace(line.BusinessId))
            {
                if (!string.Equals(line.BusinessId, request.BusinessId, StringComparison.OrdinalIgnoreCase))
                {
                    return -1;
                }

                score += 4;
            }

            if (line.AffiliationType != CCS_NpcAffiliationType.None
                && line.AffiliationType != request.PrimaryAffiliationType)
            {
                return -1;
            }

            if (line.AffiliationType == request.PrimaryAffiliationType)
            {
                score += 2;
            }

            if (line.ServiceRoute != CCS_SettlementServiceRouteType.Unknown)
            {
                if (line.ServiceRoute != request.ServiceRoute)
                {
                    return -1;
                }

                score += 6;
            }

            score += 1;
            return score;
        }

        private static bool DefinitionHasCategory(
            CCS_NpcDialogueStubDefinition definition,
            CCS_NpcDialogueStubCategory category)
        {
            CCS_NpcDialogueStubLine[] lines = definition.Lines;
            for (int index = 0; index < lines.Length; index++)
            {
                if (lines[index] != null && lines[index].Category == category)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool GlobalLinesContainCategory(
            CCS_NpcDialogueStubLine[] lines,
            CCS_NpcDialogueStubCategory category)
        {
            if (lines == null)
            {
                return false;
            }

            for (int index = 0; index < lines.Length; index++)
            {
                if (lines[index] != null && lines[index].Category == category)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendLine(List<string> lines, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            lines.Add(value);
        }

        private static CCS_NpcDialogueStubResult Fail(CCS_NpcDialogueStubResultType resultType, string message)
        {
            return new CCS_NpcDialogueStubResult
            {
                ResultType = resultType,
                Message = message ?? string.Empty
            };
        }
    }
}
