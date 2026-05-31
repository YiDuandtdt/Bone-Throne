using UnityEngine;

namespace BoneThrone.Grid
{
    /// <summary>
    /// Scene component that stores a tile coordinate and minimal traversal state.
    /// </summary>
    public sealed class Tile : MonoBehaviour
    {
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private bool walkable = true;
        [SerializeField] private bool occupied;
        [SerializeField] private int occupantId;

        public GridPosition Position
        {
            get { return new GridPosition(x, y); }
        }

        public bool IsWalkable
        {
            get { return walkable; }
        }

        public bool IsOccupied
        {
            get { return occupied; }
        }

        public int OccupantId
        {
            get { return occupantId; }
        }

        public void Initialize(GridPosition position)
        {
            x = position.X;
            y = position.Y;
        }

        public bool CanEnter()
        {
            return walkable && !occupied;
        }

        public void SetWalkable(bool value)
        {
            walkable = value;
        }

        public void SetOccupied(bool value)
        {
            occupied = value;
            if (!occupied)
            {
                occupantId = 0;
            }
        }

        public void SetOccupant(int id)
        {
            occupantId = id;
            occupied = id != 0;
        }

        public void ClearOccupant()
        {
            occupantId = 0;
            occupied = false;
        }
    }
}
