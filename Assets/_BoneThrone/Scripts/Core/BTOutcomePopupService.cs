using UnityEngine;

namespace BoneThrone.Core
{
    public sealed class BTOutcomePopupService : MonoBehaviour
    {
        private const string VictoryPopupPath = "BoneThroneUI/Outcome/UI_VictoryPopup";
        private const string DefeatPopupPath = "BoneThroneUI/Outcome/UI_DefeatPopup";

        private static BTOutcomePopupService instance;

        private GameObject activePopup;

        public static void ShowOutcome(GameOutcome outcome)
        {
            if (outcome == GameOutcome.None)
            {
                return;
            }

            EnsureInstance().ShowOutcomeInternal(outcome);
        }

        public static void HideOutcome()
        {
            if (instance != null)
            {
                instance.HideOutcomeInternal();
            }
        }

        private static BTOutcomePopupService EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            BTOutcomePopupService existing = Object.FindFirstObjectByType<BTOutcomePopupService>();
            if (existing != null)
            {
                instance = existing;
                return instance;
            }

            GameObject host = new GameObject("BTOutcomePopupService_Runtime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<BTOutcomePopupService>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void ShowOutcomeInternal(GameOutcome outcome)
        {
            string path = outcome == GameOutcome.Victory ? VictoryPopupPath : DefeatPopupPath;
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("BTOutcomePopupService could not load popup prefab at Resources path " + path + ".", this);
                return;
            }

            if (activePopup != null)
            {
                Destroy(activePopup);
            }

            activePopup = Instantiate(prefab);
        }

        private void HideOutcomeInternal()
        {
            if (activePopup != null)
            {
                Destroy(activePopup);
                activePopup = null;
            }
        }
    }
}
