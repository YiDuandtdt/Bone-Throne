using System.Collections;
using System.Collections.Generic;
using BoneThrone.Audio;
using BoneThrone.Grid;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Movement
{
    /// <summary>
    /// Applies a resolved movement path by placing the unit on the final tile.
    /// </summary>
    public sealed class UnitMover : MonoBehaviour
    {
        private static bool AnimationDebug { get { return false; } }

        [SerializeField] private Vector3 worldPositionOffset = Vector3.zero;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] [Range(0.1f, 1f)] private float bossMoveSpeedMultiplier = 0.42f;
        [SerializeField] private float turnDuration = 0.12f;

        private readonly Dictionary<Unit, Coroutine> activeMoveRoutines = new Dictionary<Unit, Coroutine>();

        public event System.Action<Unit> MoveVisualCompleted;

        private void OnDisable()
        {
            foreach (KeyValuePair<Unit, Coroutine> pair in activeMoveRoutines)
            {
                if (pair.Key != null)
                {
                    BTAudioService.StopLoop(pair.Key);
                }
            }

            StopAllCoroutines();
            activeMoveRoutines.Clear();
        }

        public bool TryMove(Unit unit, GridManager gridManager, IReadOnlyList<GridPosition> path)
        {
            if (unit == null)
            {
                Debug.LogWarning("Movement failed because Unit is missing.", this);
                return false;
            }

            if (activeMoveRoutines.ContainsKey(unit))
            {
                Debug.LogWarning("Movement rejected because unit " + unit.UnitId + " is already moving visually.", unit);
                return false;
            }

            if (gridManager == null)
            {
                Debug.LogWarning("Movement failed because GridManager is missing.", unit);
                return false;
            }

            if (path == null || path.Count < 2)
            {
                Debug.LogWarning("Movement failed because the path must include a start and target position.", unit);
                return false;
            }

            GridPosition targetPosition = path[path.Count - 1];
            Tile targetTile;
            if (!gridManager.TryGetTile(targetPosition, out targetTile))
            {
                Debug.LogWarning("Movement failed because the target tile does not exist: " + targetPosition + ".", unit);
                return false;
            }

            if (!gridManager.CanEnter(targetPosition))
            {
                Debug.LogWarning("Movement failed because the target tile cannot be entered: " + targetPosition + ".", targetTile);
                return false;
            }

            UnitAnimationController animationController = unit.GetComponent<UnitAnimationController>();
            List<Vector3> visualPath = BuildVisualPath(gridManager, path);

            Tile originalTile = unit.CurrentTile;
            bool moved = unit.TryPlaceOnTile(targetTile);
            if (!moved)
            {
                LogAnimationDebug(unit, animationController, "SetMoveSpeed(0) after failed move");
                animationController?.SetMoveSpeed(0f);
                return false;
            }

            Vector3 finalPosition = targetTile.transform.position + worldPositionOffset;
            Coroutine routine = StartCoroutine(MoveVisualRoutine(unit, visualPath, finalPosition, animationController));
            activeMoveRoutines[unit] = routine;

            if (originalTile != null && originalTile != targetTile && originalTile.IsOccupied)
            {
                Debug.LogWarning("Movement completed, but the original tile still appears occupied: " + originalTile.Position + ".", originalTile);
            }

            if (!targetTile.IsOccupied || targetTile.OccupantId != unit.UnitId)
            {
                Debug.LogWarning("Movement completed, but target occupancy does not match UnitId " + unit.UnitId + ".", targetTile);
            }

            Debug.Log("Moved unit " + unit.UnitId + " to " + targetPosition + ".", unit);
            return true;
        }

        private List<Vector3> BuildVisualPath(GridManager gridManager, IReadOnlyList<GridPosition> path)
        {
            List<Vector3> visualPath = new List<Vector3>();
            if (gridManager == null || path == null)
            {
                return visualPath;
            }

            for (int i = 1; i < path.Count; i++)
            {
                Tile tile;
                if (gridManager.TryGetTile(path[i], out tile) && tile != null)
                {
                    visualPath.Add(tile.transform.position + worldPositionOffset);
                }
            }

            return visualPath;
        }

        private IEnumerator MoveVisualRoutine(
            Unit unit,
            IReadOnlyList<Vector3> visualPath,
            Vector3 finalPosition,
            UnitAnimationController animationController)
        {
            if (unit == null)
            {
                yield break;
            }

            LogAnimationDebug(unit, animationController, "SetMoveSpeed(1)");
            animationController?.SetMoveSpeed(1f);
            Object audioOwner = unit;
            BTAudioService.PlayLoop(BTAudioService.GetFootstepCue(unit), audioOwner);
            float resolvedMoveSpeed = GetMoveSpeed(unit);

            if (visualPath == null || visualPath.Count == 0)
            {
                visualPath = new[] { finalPosition };
            }

            for (int i = 0; i < visualPath.Count; i++)
            {
                Vector3 segmentTarget = visualPath[i];
                yield return RotateTowardSegment(unit, segmentTarget);
                yield return MoveSegment(unit, segmentTarget, resolvedMoveSpeed);
            }

            if (unit != null)
            {
                unit.transform.position = finalPosition;
                animationController?.SetMoveSpeed(0f);
                Unit nearestOpponent = FindNearestAliveOpponent(unit);
                if (nearestOpponent != null)
                {
                    yield return RotateTowardSegment(unit, nearestOpponent.transform.position);
                }
            }

            BTAudioService.StopLoop(audioOwner);
            activeMoveRoutines.Remove(unit);
            MoveVisualCompleted?.Invoke(unit);
        }

        private IEnumerator RotateTowardSegment(Unit unit, Vector3 segmentTarget)
        {
            if (unit == null)
            {
                yield break;
            }

            Vector3 delta = segmentTarget - unit.transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude <= 0.0001f)
            {
                yield break;
            }

            Quaternion startRotation = unit.transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(GetCardinalDirection(delta), Vector3.up);
            float duration = Mathf.Max(0f, turnDuration);
            if (duration <= 0f)
            {
                unit.transform.rotation = targetRotation;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (unit == null)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                unit.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            unit.transform.rotation = targetRotation;
        }

        private IEnumerator MoveSegment(Unit unit, Vector3 segmentTarget, float resolvedMoveSpeed)
        {
            if (unit == null)
            {
                yield break;
            }

            float speed = Mathf.Max(0.01f, resolvedMoveSpeed);
            while (unit != null && Vector3.Distance(unit.transform.position, segmentTarget) > 0.01f)
            {
                unit.transform.position = Vector3.MoveTowards(unit.transform.position, segmentTarget, speed * Time.deltaTime);
                yield return null;
            }

            if (unit != null)
            {
                unit.transform.position = segmentTarget;
            }
        }

        private float GetMoveSpeed(Unit unit)
        {
            float speed = Mathf.Max(0.01f, moveSpeed);
            if (IsBossLikeUnit(unit))
            {
                return speed * Mathf.Clamp(bossMoveSpeedMultiplier, 0.1f, 1f);
            }

            return speed;
        }

        private static bool IsBossLikeUnit(Unit unit)
        {
            if (unit == null)
            {
                return false;
            }

            string objectName = unit.name != null ? unit.name.ToLowerInvariant() : string.Empty;
            string displayName = unit.DisplayName != null ? unit.DisplayName.ToLowerInvariant() : string.Empty;
            return objectName.Contains("boss")
                || objectName.Contains("golem")
                || objectName.Contains("large")
                || displayName.Contains("boss")
                || displayName.Contains("golem")
                || displayName.Contains("large");
        }

        private Vector3 GetCardinalDirection(Vector3 worldDelta)
        {
            if (Mathf.Abs(worldDelta.x) >= Mathf.Abs(worldDelta.z))
            {
                return worldDelta.x >= 0f ? Vector3.right : Vector3.left;
            }

            return worldDelta.z >= 0f ? Vector3.forward : Vector3.back;
        }

        private Unit FindNearestAliveOpponent(Unit unit)
        {
            if (unit == null)
            {
                return null;
            }

            UnitFaction targetFaction;
            if (unit.Faction == UnitFaction.Player)
            {
                targetFaction = UnitFaction.Enemy;
            }
            else if (unit.Faction == UnitFaction.Enemy)
            {
                targetFaction = UnitFaction.Player;
            }
            else
            {
                return null;
            }

            Unit[] units = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
            Unit nearest = null;
            float nearestDistanceSq = float.MaxValue;
            for (int i = 0; i < units.Length; i++)
            {
                Unit candidate = units[i];
                if (candidate == null || candidate == unit || !candidate.IsAlive || candidate.Faction != targetFaction)
                {
                    continue;
                }

                Vector3 delta = candidate.transform.position - unit.transform.position;
                delta.y = 0f;
                float distanceSq = delta.sqrMagnitude;
                if (distanceSq < nearestDistanceSq)
                {
                    nearestDistanceSq = distanceSq;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        private void LogAnimationDebug(Unit unit, UnitAnimationController animationController, string method)
        {
            if (!AnimationDebug)
            {
                return;
            }

            Debug.Log(
                "AnimationDebug UnitMover: unit="
                + (unit != null ? unit.name : "null")
                + " UnitId="
                + (unit != null ? unit.UnitId.ToString() : "n/a")
                + " controller="
                + (animationController != null ? animationController.name : "null")
                + " method="
                + method
                + ".",
                unit);
        }
    }
}
