using BoneThrone.Audio;
using BoneThrone.Levels;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Interactables
{
    /// <summary>
    /// Minimal boss-gate key pickup. It is separate from ordinary shared floor keys.
    /// </summary>
    public sealed class BossKeyItem : MonoBehaviour
    {
        [SerializeField] private BossGateProgressionState progressionState;
        [SerializeField] private bool consumeOnCollect = true;
        [SerializeField] private bool autoCollectWhenPlayerNearby = true;
        [SerializeField] [Min(0.1f)] private float pickupRadius = 1.6f;
        [SerializeField] [Min(0.05f)] private float pickupCheckInterval = 0.2f;
        [SerializeField] private bool collected;
        [SerializeField] private bool debugLogging;

        private bool missingProgressionWarningLogged;
        private float nextPickupCheckTime;

        public bool IsCollected
        {
            get { return collected; }
        }

        private void OnMouseDown()
        {
            TryCollect(true);
        }

        public bool TryCollect()
        {
            return TryCollect(false);
        }

        private void Update()
        {
            if (!autoCollectWhenPlayerNearby || collected || Time.time < nextPickupCheckTime)
            {
                return;
            }

            nextPickupCheckTime = Time.time + Mathf.Max(0.05f, pickupCheckInterval);
            if (!CanAttemptCollectNow())
            {
                return;
            }

            Unit nearbyPlayer = FindNearbyLivingPlayer();
            if (nearbyPlayer != null)
            {
                TryCollect(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || collected)
            {
                return;
            }

            Unit collector = other.GetComponentInParent<Unit>();
            if (collector == null || collector.Faction != UnitFaction.Player || !collector.IsAlive)
            {
                return;
            }

            TryCollect(false);
        }

        private bool TryCollect(bool playRejectedAudio)
        {
            if (collected)
            {
                Log("Collection ignored because this boss key is already collected.");
                return false;
            }

            ResolveProgressionState();
            if (progressionState == null)
            {
                LogMissingProgressionWarning();
                if (playRejectedAudio)
                {
                    BTAudioService.PlaySfx(BTAudioCueId.InvalidAction);
                }

                return false;
            }

            if (!progressionState.CanCollectBossKey())
            {
                Log("Collection rejected because boss gate requirements are not met.");
                if (playRejectedAudio)
                {
                    BTAudioService.PlaySfx(BTAudioCueId.InvalidAction);
                }

                return false;
            }

            bool collectedByState = progressionState.CollectBossKey();
            if (!collectedByState)
            {
                return false;
            }

            collected = true;
            Log("Boss key collected.");
            BTAudioService.PlaySfx(BTAudioCueId.KeyPickup);
            BTInteractionVfxService.PlayKeyPickup(transform.position);

            if (consumeOnCollect)
            {
                gameObject.SetActive(false);
            }

            return true;
        }

        private bool CanAttemptCollectNow()
        {
            ResolveProgressionState();
            return progressionState != null && progressionState.CanCollectBossKey();
        }

        private Unit FindNearbyLivingPlayer()
        {
            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            float radiusSqr = pickupRadius * pickupRadius;
            Vector3 keyPosition = transform.position;
            for (int i = 0; i < units.Length; i++)
            {
                Unit unit = units[i];
                if (unit == null
                    || !unit.gameObject.activeInHierarchy
                    || unit.Faction != UnitFaction.Player
                    || !unit.IsAlive)
                {
                    continue;
                }

                Vector3 delta = unit.transform.position - keyPosition;
                delta.y = 0f;
                if (delta.sqrMagnitude <= radiusSqr)
                {
                    return unit;
                }
            }

            return null;
        }

        private void ResolveProgressionState()
        {
            if (progressionState == null)
            {
                progressionState = BossGateProgressionState.GetOrCreateSceneState();
            }
        }

        private void LogMissingProgressionWarning()
        {
            if (missingProgressionWarningLogged)
            {
                return;
            }

            missingProgressionWarningLogged = true;
            Debug.LogWarning("BossKeyItem cannot be collected because BossGateProgressionState is missing.", this);
        }

        private void Log(string message)
        {
            if (debugLogging)
            {
                Debug.Log("BossKeyItem: " + message, this);
            }
        }
    }
}
