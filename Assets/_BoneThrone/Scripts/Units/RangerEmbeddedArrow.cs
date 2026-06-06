using System.Collections;
using BoneThrone.Turns;
using UnityEngine;

namespace BoneThrone.Units
{
    /// <summary>
    /// Handles a short impact flight, then keeps the arrow embedded on the target for a limited number of enemy turns.
    /// </summary>
    public sealed class RangerEmbeddedArrow : MonoBehaviour
    {
        private Transform targetAnchor;
        private Unit targetUnit;
        private Vector3 embeddedLocalOffset;
        private Quaternion worldRotation;
        private TurnManager turnManager;
        private float fallbackLifetimeSeconds;
        private int remainingEnemyTurns;
        private bool embedded;

        public void Initialize(
            Vector3 startWorldPosition,
            Transform targetAnchorTransform,
            Vector3 localOffset,
            Quaternion rotation,
            int enemyTurnsToLive,
            float fallbackLifetime,
            float flightDuration,
            TurnManager sourceTurnManager)
        {
            targetAnchor = targetAnchorTransform;
            targetUnit = targetAnchor != null ? targetAnchor.GetComponentInParent<Unit>() : null;
            embeddedLocalOffset = localOffset;
            worldRotation = rotation;
            turnManager = sourceTurnManager;
            fallbackLifetimeSeconds = Mathf.Max(0.1f, fallbackLifetime);
            remainingEnemyTurns = Mathf.Max(1, enemyTurnsToLive);

            transform.position = startWorldPosition;
            transform.rotation = worldRotation;

            StopAllCoroutines();
            StartCoroutine(FlyToTarget(flightDuration));
        }

        private void OnDestroy()
        {
            UnsubscribeTurnManager();
        }

        private IEnumerator FlyToTarget(float flightDuration)
        {
            float duration = Mathf.Max(0f, flightDuration);
            Vector3 startPosition = transform.position;

            if (duration > 0f)
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    transform.position = Vector3.Lerp(startPosition, ResolveImpactWorldPosition(), t);
                    transform.rotation = worldRotation;
                    yield return null;
                }
            }

            if (targetAnchor == null)
            {
                Destroy(gameObject, fallbackLifetimeSeconds);
                yield break;
            }

            transform.position = ResolveImpactWorldPosition();
            transform.rotation = worldRotation;
            transform.SetParent(targetAnchor, true);
            embedded = true;

            if (turnManager != null)
            {
                turnManager.EnemyTurnStarted += HandleEnemyTurnStarted;
            }
            else
            {
                Destroy(gameObject, fallbackLifetimeSeconds);
            }
        }

        private Vector3 ResolveImpactWorldPosition()
        {
            if (targetAnchor == null)
            {
                return transform.position;
            }

            return targetAnchor.TransformPoint(embeddedLocalOffset);
        }

        private void HandleEnemyTurnStarted()
        {
            if (!embedded)
            {
                return;
            }

            if (targetUnit == null || !targetUnit.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            remainingEnemyTurns--;
            if (remainingEnemyTurns <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void UnsubscribeTurnManager()
        {
            if (turnManager != null)
            {
                turnManager.EnemyTurnStarted -= HandleEnemyTurnStarted;
            }
        }
    }
}
