using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoneThrone.UI
{
    /// <summary>
    /// Shared binding surface for the reusable full-screen settings page prefab.
    /// Layout stays owned by the prefab; controllers only bind actions and audio values.
    /// </summary>
    public sealed class SettingsPageView : MonoBehaviour
    {
        private const float DefaultSliderZeroSnapThreshold = 0.01f;

        [Header("Sliders")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Image bgmFillImage;
        [SerializeField] private Image sfxFillImage;

        [SerializeField] [Range(0f, 0.05f)] private float sliderZeroSnapThreshold = DefaultSliderZeroSnapThreshold;

        [Header("Buttons")]
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TMP_Text primaryButtonLabel;
        [SerializeField] private TMP_Text secondaryButtonLabel;

        private bool sliderListenersBound;
        private bool snappingSliderValue;

        public Slider BgmSlider
        {
            get
            {
                EnsureViewConfiguration();
                return bgmSlider;
            }
        }

        public Slider SfxSlider
        {
            get
            {
                EnsureViewConfiguration();
                return sfxSlider;
            }
        }

        public Button PrimaryButton
        {
            get
            {
                EnsureViewConfiguration();
                return primaryButton;
            }
        }

        public Button SecondaryButton
        {
            get
            {
                EnsureViewConfiguration();
                return secondaryButton;
            }
        }

        private void Awake()
        {
            EnsureViewConfiguration();
        }

        private void OnEnable()
        {
            EnsureViewConfiguration();
            BindSliderZeroSnapListeners();
        }

        private void OnDisable()
        {
            UnbindSliderZeroSnapListeners();
        }

        private void Reset()
        {
            EnsureViewConfiguration();
        }

        private void OnValidate()
        {
            EnsureViewConfiguration();
        }

        public static float SnapAudioValue(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return clampedValue <= DefaultSliderZeroSnapThreshold ? 0f : clampedValue;
        }

        public void SetAudioValuesWithoutNotify(float bgmValue, float sfxValue)
        {
            EnsureViewConfiguration();
            SetSliderValueWithoutNotify(bgmSlider, bgmValue);
            SetSliderValueWithoutNotify(sfxSlider, sfxValue);
        }

        public void SetButtonLabels(string primaryText, string secondaryText)
        {
            EnsureViewConfiguration();

            if (primaryButtonLabel != null)
            {
                primaryButtonLabel.text = primaryText;
            }

            if (secondaryButtonLabel != null)
            {
                secondaryButtonLabel.text = secondaryText;
            }
        }

        private void EnsureViewConfiguration()
        {
            CacheMissingReferences();
            ResolveAudioSlidersByVerticalOrder();
            ConfigureAudioSlider(sfxSlider);
            ConfigureAudioSlider(bgmSlider);
        }

        private void CacheMissingReferences()
        {
            if (bgmSlider == null)
            {
                bgmSlider = FindNamedComponent<Slider>("BgmSlider");
            }

            if (sfxSlider == null)
            {
                sfxSlider = FindNamedComponent<Slider>("SfxSlider");
            }

            if (bgmFillImage == null)
            {
                bgmFillImage = FindSliderFillImage(bgmSlider);
            }

            if (sfxFillImage == null)
            {
                sfxFillImage = FindSliderFillImage(sfxSlider);
            }

            if (primaryButton == null)
            {
                primaryButton = FindNamedComponent<Button>("PrimaryButton");
            }

            if (secondaryButton == null)
            {
                secondaryButton = FindNamedComponent<Button>("SecondaryButton");
            }

            if (primaryButtonLabel == null)
            {
                primaryButtonLabel = FindNamedComponent<TMP_Text>("PrimaryButtonLabel");
            }

            if (secondaryButtonLabel == null)
            {
                secondaryButtonLabel = FindNamedComponent<TMP_Text>("SecondaryButtonLabel");
            }
        }

        private void ResolveAudioSlidersByVerticalOrder()
        {
            Slider[] sliders = GetComponentsInChildren<Slider>(true);
            if (sliders.Length < 2)
            {
                return;
            }

            Slider topSlider = null;
            Slider bottomSlider = null;
            float topY = float.NegativeInfinity;
            float bottomY = float.PositiveInfinity;

            for (int i = 0; i < sliders.Length; i++)
            {
                Slider slider = sliders[i];
                if (slider == null)
                {
                    continue;
                }

                RectTransform sliderRect = slider.transform as RectTransform;
                float sliderY = sliderRect != null ? sliderRect.anchoredPosition.y : slider.transform.localPosition.y;

                if (sliderY > topY)
                {
                    topY = sliderY;
                    topSlider = slider;
                }

                if (sliderY < bottomY)
                {
                    bottomY = sliderY;
                    bottomSlider = slider;
                }
            }

            if (topSlider == null || bottomSlider == null || topSlider == bottomSlider)
            {
                return;
            }

            sfxSlider = topSlider;
            bgmSlider = bottomSlider;
        }

        private void ConfigureAudioSlider(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            slider.gameObject.SetActive(true);
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;

            ConfigureSliderFillImage(slider);

            slider.SetValueWithoutNotify(Mathf.Clamp01(slider.value));
            RefreshSliderFillImage(slider);
        }

        private void SetSliderValueWithoutNotify(Slider slider, float value)
        {
            if (slider == null)
            {
                return;
            }

            slider.SetValueWithoutNotify(SnapAudioValue(value));
            RefreshSliderFillImage(slider);
        }

        private void ConfigureSliderFillImage(Slider slider)
        {
            Image fillImage = GetFillImageForSlider(slider);
            if (fillImage == null)
            {
                return;
            }

            fillImage.gameObject.SetActive(true);
            fillImage.raycastTarget = false;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillClockwise = true;
        }

        private void RefreshSliderFillImage(Slider slider)
        {
            Image fillImage = GetFillImageForSlider(slider);
            if (fillImage == null || slider == null)
            {
                return;
            }

            fillImage.fillAmount = Mathf.Clamp01(slider.normalizedValue);
        }

        private Image GetFillImageForSlider(Slider slider)
        {
            if (slider == sfxSlider)
            {
                return sfxFillImage;
            }

            if (slider == bgmSlider)
            {
                return bgmFillImage;
            }

            return FindSliderFillImage(slider);
        }

        private void BindSliderZeroSnapListeners()
        {
            if (sliderListenersBound)
            {
                return;
            }

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.AddListener(HandleBgmSliderValueChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.AddListener(HandleSfxSliderValueChanged);
            }

            sliderListenersBound = true;
        }

        private void UnbindSliderZeroSnapListeners()
        {
            if (!sliderListenersBound)
            {
                return;
            }

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.RemoveListener(HandleBgmSliderValueChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveListener(HandleSfxSliderValueChanged);
            }

            sliderListenersBound = false;
        }

        private void HandleBgmSliderValueChanged(float value)
        {
            SnapSliderValueIfNeeded(bgmSlider, value);
            RefreshSliderFillImage(bgmSlider);
        }

        private void HandleSfxSliderValueChanged(float value)
        {
            SnapSliderValueIfNeeded(sfxSlider, value);
            RefreshSliderFillImage(sfxSlider);
        }

        private void SnapSliderValueIfNeeded(Slider slider, float value)
        {
            if (snappingSliderValue || slider == null)
            {
                return;
            }

            float clampedValue = Mathf.Clamp01(value);
            if (clampedValue <= 0f || clampedValue > sliderZeroSnapThreshold)
            {
                return;
            }

            snappingSliderValue = true;
            slider.value = 0f;
            snappingSliderValue = false;
        }

        private T FindNamedComponent<T>(string objectName) where T : Component
        {
            T[] components = GetComponentsInChildren<T>(true);
            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component != null && component.gameObject.name == objectName)
                {
                    return component;
                }
            }

            return null;
        }

        private static Image FindSliderFillImage(Slider slider)
        {
            if (slider == null)
            {
                return null;
            }

            Image[] images = slider.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image != null && image.gameObject.name == "Fill")
                {
                    return image;
                }
            }

            return null;
        }
    }
}
