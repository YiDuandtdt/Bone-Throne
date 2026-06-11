using UnityEngine;

namespace BoneThrone.Rooms
{
    /// <summary>
    /// Shows or hides a simple semi-transparent room shadow root.
    /// This is not grid-based fog of war or a render feature.
    /// </summary>
    public sealed class RoomShadowController : MonoBehaviour
    {
        [SerializeField] private GameObject shadowRoot;

        public bool IsShadowVisible
        {
            get { return shadowRoot != null && shadowRoot.activeSelf; }
        }

        public void ShowShadow()
        {
            SetShadowVisible(true);
        }

        public void HideShadow()
        {
            SetShadowVisible(false);
        }

        public void SetShadowVisible(bool visible)
        {
            if (shadowRoot == null)
            {
                Debug.LogWarning("RoomShadowController cannot change visibility because shadowRoot is missing.", this);
                return;
            }

            shadowRoot.SetActive(visible);
        }
    }
}
