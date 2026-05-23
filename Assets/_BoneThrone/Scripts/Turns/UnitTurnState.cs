using UnityEngine;

namespace BoneThrone.Turns
{
    /// <summary>
    /// Lightweight per-unit turn flags for movement and action consumption.
    /// It does not execute attacks, skills, items, defense, AI, or networking.
    /// </summary>
    public sealed class UnitTurnState : MonoBehaviour
    {
        [SerializeField] private bool hasMoved;
        [SerializeField] private bool hasActed;

        public bool HasMoved
        {
            get { return hasMoved; }
        }

        public bool HasActed
        {
            get { return hasActed; }
        }

        public void ResetForNewRound()
        {
            hasMoved = false;
            hasActed = false;
        }

        public void MarkMoved()
        {
            hasMoved = true;
        }

        public void MarkActed()
        {
            hasActed = true;
        }
    }
}
