using BoneThrone.Units;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Read-only world-space health bar for enemy units.
    /// </summary>
    public sealed class EnemyFloatingHealthBarView : MonoBehaviour
    {
        private static readonly Color HealthFillColor = new Color(0.9f, 0.03f, 0.02f, 1f);

        [SerializeField] private Unit unit;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Canvas worldCanvas;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform fillRect;
        [SerializeField] private Image missingFillImage;
        [SerializeField] private RectTransform missingFillRect;
        [SerializeField] private GameObject barRoot;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.1f, 0f);

        private int lastHp = int.MinValue;
        private int lastMaxHp = int.MinValue;
        private bool lastAlive;

        private void Awake()
        {
            ResolveReferences();
            DisableRaycastTargets();
            ForceRefresh();
        }

        private void OnEnable()
        {
            ResolveReferences();
            ConfigureFillRect();
            ForceRefresh();
        }

        private void Update()
        {
            ResolveReferences();
            ConfigureFillRect();
            RefreshIfNeeded();
        }

        private void LateUpdate()
        {
            if (unit != null)
            {
                transform.position = unit.transform.position + worldOffset;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                return;
            }

            transform.rotation = targetCamera.transform.rotation;
        }

        private void ResolveReferences()
        {
            if (unit == null)
            {
                unit = GetComponentInParent<Unit>();
            }
            else
            {
                Unit parentUnit = GetComponentInParent<Unit>();
                if (parentUnit != null && parentUnit != unit)
                {
                    unit = parentUnit;
                }
            }

            if (worldCanvas == null)
            {
                worldCanvas = GetComponentInChildren<Canvas>(true);
            }

            if (barRoot == null && worldCanvas != null)
            {
                barRoot = worldCanvas.gameObject;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (fillImage == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] != null && images[i].gameObject.name == "Fill")
                    {
                        fillImage = images[i];
                        break;
                    }
                }
            }

            if (fillRect == null && fillImage != null)
            {
                fillRect = fillImage.rectTransform;
            }

            if (missingFillImage == null)
            {
                Image[] images = GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] != null && (images[i].gameObject.name == "MissingFill" || images[i].gameObject.name == "FillBackground"))
                    {
                        missingFillImage = images[i];
                        break;
                    }
                }
            }

            if (missingFillRect == null && missingFillImage != null)
            {
                missingFillRect = missingFillImage.rectTransform;
            }
        }

        private void DisableRaycastTargets()
        {
            if (worldCanvas != null)
            {
                GraphicRaycaster raycaster = worldCanvas.GetComponent<GraphicRaycaster>();
                if (raycaster != null)
                {
                    raycaster.enabled = false;
                }
            }

            if (backgroundImage != null)
            {
                backgroundImage.raycastTarget = false;
            }

            if (fillImage != null)
            {
                fillImage.color = HealthFillColor;
                fillImage.raycastTarget = false;
                fillImage.type = Image.Type.Simple;
            }

            if (missingFillImage != null)
            {
                missingFillImage.raycastTarget = false;
                missingFillImage.type = Image.Type.Simple;
            }

            ConfigureFillRect();
        }

        private void ConfigureFillRect()
        {
            if (fillRect == null)
            {
                return;
            }

            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = Vector2.zero;

            if (missingFillRect != null)
            {
                missingFillRect.pivot = new Vector2(0f, 0.5f);
                missingFillRect.anchoredPosition = Vector2.zero;
                missingFillRect.sizeDelta = Vector2.zero;
            }
        }

        private void ForceRefresh()
        {
            lastHp = int.MinValue;
            lastMaxHp = int.MinValue;
            RefreshIfNeeded();
        }

        private void RefreshIfNeeded()
        {
            if (unit == null || unit.RuntimeState == null || unit.Stats == null)
            {
                SetBarVisible(false);
                return;
            }

            int hp = Mathf.Max(0, unit.RuntimeState.CurrentHp);
            int maxHp = Mathf.Max(1, unit.Stats.GetClampedMaxHp());
            bool alive = unit.IsAlive;
            if (!alive)
            {
                SetBarVisible(false);
                lastHp = hp;
                lastMaxHp = maxHp;
                lastAlive = false;
                return;
            }

            SetBarVisible(true);
            if (hp == lastHp && maxHp == lastMaxHp && alive == lastAlive)
            {
                return;
            }

            if (fillImage != null)
            {
                fillImage.raycastTarget = false;
                float ratio = Mathf.Clamp01((float)hp / maxHp);
                ApplyFill(ratio);
            }

            lastHp = hp;
            lastMaxHp = maxHp;
            lastAlive = alive;
        }

        private void ApplyFill(float ratio)
        {
            float clampedRatio = Mathf.Clamp01(ratio);

            if (fillRect != null)
            {
                fillRect.anchorMax = new Vector2(clampedRatio, 1f);
            }

            if (missingFillRect != null)
            {
                missingFillRect.anchorMin = new Vector2(clampedRatio, 0f);
                missingFillRect.anchorMax = Vector2.one;
            }
        }

        private void SetBarVisible(bool visible)
        {
            if (worldCanvas != null && worldCanvas.enabled != visible)
            {
                worldCanvas.enabled = visible;
            }

            if (barRoot != null && barRoot != gameObject && barRoot.activeSelf != visible)
            {
                barRoot.SetActive(visible);
            }
        }
    }
}
