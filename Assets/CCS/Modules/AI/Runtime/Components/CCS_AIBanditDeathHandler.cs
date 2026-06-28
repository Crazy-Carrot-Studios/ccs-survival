using CCS.Modules.Attributes;

using Unity.Netcode;

using UnityEngine;

using UnityEngine.AI;



// =============================================================================

// SCRIPT: CCS_AIBanditDeathHandler

// CATEGORY: Modules / AI / Runtime / Components

// PURPOSE: Stops AI combat/movement on death, enables ragdoll, and despawns after delay.

// PLACEMENT: AI bandit root with CCS_NetworkHealth and NetworkObject.

// AUTHOR: James Schilz

// CREATED: 2026-06-25

// NOTES: v0.7.1 ragdoll death feedback with server-authoritative despawn.

// =============================================================================



namespace CCS.Modules.AI

{

    [DefaultExecutionOrder(70)]

    public sealed class CCS_AIBanditDeathHandler : MonoBehaviour

    {

        [SerializeField] private float deathDespawnDelay = 4f;

        [SerializeField] private bool enableRagdollOnDeath = true;

        [SerializeField] private bool hideNameplateOnDeath = false;

        [SerializeField] private bool despawnOnDeath = true;

        [SerializeField] private float ragdollImpulse = 2.5f;

        [SerializeField] private CCS_NetworkHealth networkHealth;

        [SerializeField] private CCS_AIBanditBrain brain;

        [SerializeField] private CCS_AIMotorController motorController;

        [SerializeField] private CCS_AIAnimatorDriver animatorDriver;

        [SerializeField] private CCS_AIWeaponController weaponController;

        [SerializeField] private CCS_RagdollController ragdollController;

        [SerializeField] private NavMeshAgent navMeshAgent;

        [SerializeField] private UnityEngine.CharacterController characterController;

        [SerializeField] private bool enableDeathDebugLogs;



        private bool deathStarted;

        private bool removalScheduled;

        private Coroutine deathRoutine;



        public bool DeathStarted => deathStarted;



        private void Awake()

        {

            ResolveReferences();

        }



        private void OnEnable()

        {

            ResolveReferences();

            BindHealthEvents();

            EvaluateDeathState();

        }



        private void OnDisable()

        {

            UnbindHealthEvents();

        }



        private void Update()

        {

            EvaluateDeathState();

        }



        private void LateUpdate()

        {

            EvaluateDeathState();

        }



        public void ForceDeathHandling()

        {

            EvaluateDeathState();

        }



        public void ForceDeathForValidation(bool immediateRemoval = false)

        {

            ResolveReferences();

            if (networkHealth == null)

            {

                return;

            }



            if (!deathStarted)

            {

                BeginDeathFeedback();

            }



            if (immediateRemoval)

            {

                RemoveFromSceneImmediate();

            }

            else if (!removalScheduled)

            {

                ScheduleRemovalAfterDelay();

            }

        }



        private void BindHealthEvents()

        {

            if (networkHealth == null)

            {

                return;

            }



            networkHealth.HealthChanged -= HandleHealthChanged;

            networkHealth.HealthChanged += HandleHealthChanged;

            networkHealth.DeadStateChanged -= HandleDeadStateChanged;

            networkHealth.DeadStateChanged += HandleDeadStateChanged;

        }



        private void UnbindHealthEvents()

        {

            if (networkHealth == null)

            {

                return;

            }



            networkHealth.HealthChanged -= HandleHealthChanged;

            networkHealth.DeadStateChanged -= HandleDeadStateChanged;

        }



        private void HandleHealthChanged(float currentHealth, float maxHealth)

        {

            EvaluateDeathState();

        }



        private void HandleDeadStateChanged(bool isDead)

        {

            EvaluateDeathState();

        }



        private void EvaluateDeathState()

        {

            ResolveReferences();

            if (networkHealth == null || !networkHealth.IsDead)

            {

                return;

            }



            if (!deathStarted)

            {

                BeginDeathFeedback();

            }



            if (!removalScheduled && IsRemovalAuthorityActive())

            {

                ScheduleRemovalAfterDelay();

            }

        }



        private void BeginDeathFeedback()

        {

            if (deathStarted)

            {

                return;

            }



            deathStarted = true;

            brain?.ForceDeadState();

            weaponController?.SetAimHeld(false);

            motorController?.Stop();

            animatorDriver?.StopDriving();



            if (navMeshAgent != null)

            {

                navMeshAgent.isStopped = true;

                navMeshAgent.ResetPath();

                navMeshAgent.enabled = false;

            }



            if (characterController != null)

            {

                characterController.enabled = false;

            }



            if (enableRagdollOnDeath && ragdollController != null)

            {

                Vector3 impulseDirection = networkHealth != null ? networkHealth.LastDamageDirection : Vector3.zero;

                if (impulseDirection.sqrMagnitude <= 0.0001f)

                {

                    impulseDirection = -transform.forward;

                }



                ragdollController.EnableRagdoll(impulseDirection, ragdollImpulse);

            }

            else if (ragdollController != null)

            {

                ragdollController.SetRagdollActive(false);

            }



            CCS_AIBanditNameplate nameplate = GetComponent<CCS_AIBanditNameplate>();

            if (nameplate != null)

            {

                nameplate.SetHealthPercent(0f);

                if (hideNameplateOnDeath)

                {

                    nameplate.SetNameplateVisible(false);

                }

            }



            CCS_AIBanditDamageHitbox damageHitbox = GetComponentInChildren<CCS_AIBanditDamageHitbox>(true);

            damageHitbox?.SetHitboxEnabled(false);



            if (enableDeathDebugLogs)

            {

                Debug.Log("[AI Bandit] Death feedback started.", this);

            }

        }



        private void ScheduleRemovalAfterDelay()

        {

            removalScheduled = true;

            if (deathRoutine != null)

            {

                StopCoroutine(deathRoutine);

            }



            deathRoutine = StartCoroutine(RemoveAfterDelayRoutine());

        }



        private System.Collections.IEnumerator RemoveAfterDelayRoutine()

        {

            float delay = Mathf.Max(0f, deathDespawnDelay);

            if (delay > 0f)

            {

                yield return new WaitForSecondsRealtime(delay);

            }



            RemoveFromSceneImmediate();

        }



        private void RemoveFromSceneImmediate()

        {

            if (!despawnOnDeath)

            {

                return;

            }



            NetworkObject networkObject = GetComponent<NetworkObject>();

            if (networkObject != null

                && NetworkManager.Singleton != null

                && NetworkManager.Singleton.IsListening

                && networkObject.IsSpawned

                && NetworkManager.Singleton.IsServer)

            {

                networkObject.Despawn(true);

                return;

            }



            if (IsRemovalAuthorityActive())

            {

                if (Application.isPlaying)

                {

                    Destroy(gameObject);

                }

                else

                {

                    DestroyImmediate(gameObject);

                }

            }

        }



        private bool IsRemovalAuthorityActive()

        {

            NetworkObject networkObject = GetComponent<NetworkObject>();

            if (networkObject == null

                || NetworkManager.Singleton == null

                || !NetworkManager.Singleton.IsListening)

            {

                return true;

            }



            return NetworkManager.Singleton.IsServer;

        }



        private void ResolveReferences()

        {

            if (networkHealth == null)

            {

                networkHealth = GetComponent<CCS_NetworkHealth>();

            }



            if (brain == null)

            {

                brain = GetComponent<CCS_AIBanditBrain>();

            }



            if (motorController == null)

            {

                motorController = GetComponent<CCS_AIMotorController>();

            }



            if (animatorDriver == null)

            {

                animatorDriver = GetComponent<CCS_AIAnimatorDriver>();

            }



            if (weaponController == null)

            {

                weaponController = GetComponent<CCS_AIWeaponController>();

            }



            if (ragdollController == null)

            {

                ragdollController = GetComponent<CCS_RagdollController>();

            }



            if (navMeshAgent == null)

            {

                navMeshAgent = GetComponent<NavMeshAgent>();

            }



            if (characterController == null)

            {

                characterController = GetComponent<UnityEngine.CharacterController>();

            }

        }

    }

}


