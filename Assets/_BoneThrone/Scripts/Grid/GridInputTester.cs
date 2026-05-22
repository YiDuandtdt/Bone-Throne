using UnityEngine;

namespace BoneThrone.Grid
{
    /// <summary>
    /// Temporary debug helper for verifying tile click coordinates in Play Mode.
    /// </summary>
    public sealed class GridInputTester : MonoBehaviour
    {
        [SerializeField] private Camera inputCamera;
        [SerializeField] private LayerMask tileLayerMask = ~0;
        [SerializeField] private float maxRayDistance = 500f;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            Camera cameraToUse = inputCamera != null ? inputCamera : Camera.main;
            if (cameraToUse == null)
            {
                Debug.LogWarning("GridInputTester needs an input camera or a MainCamera-tagged camera.", this);
                return;
            }

            Ray ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, maxRayDistance, tileLayerMask))
            {
                return;
            }

            Tile tile = hit.collider.GetComponentInParent<Tile>();
            if (tile == null)
            {
                return;
            }

            Debug.Log("Clicked tile " + tile.Position + " Walkable=" + tile.IsWalkable + " Occupied=" + tile.IsOccupied, tile);
        }
    }
}
