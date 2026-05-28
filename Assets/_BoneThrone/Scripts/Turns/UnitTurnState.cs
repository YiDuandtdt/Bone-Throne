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
        [SerializeField] private bool hasEnded;

        public bool HasMoved
        {
            get { return hasMoved; }
        }

        public bool HasActed
        {
            get { return hasActed; }
        }

        public bool HasEnded
        {
            get { return hasEnded; }
        }

        public void ResetForNewRound()
        {
            hasMoved = false;
            hasActed = false;
            hasEnded = false;
        }

        public void MarkMoved()
        {
            hasMoved = true;
        }

        public void MarkActed()
        {
            hasActed = true;
        }

        public void MarkEnded()
        {
            hasEnded = true;
        }
    }
}
