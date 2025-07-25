#if DOTWEEN
using DG.Tweening;
#endif
using IceColdCore.UI.Interface;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IceColdCore.UI
{
    //todo add button sounds, a default or a custom, default defined somewhere, button would take a sound file to play
    public class CustomButton : Button
    {
        [Header("Colours")]
        [SerializeField] protected Color onColour = Color.white;
        [SerializeField] protected Color offColour = Color.white;
        
        [Header("Flip direction")]
        [SerializeField] protected FlipDirection flipDirection = FlipDirection.Vertical;
        
        [Header("Button texts")]
        [SerializeField] private TextMeshProUGUI label;

        [Header("Indicator")]
        [SerializeField] private GameObject indicator;
        [SerializeField] private TextMeshProUGUI indicatorLabel;

        [Header("Icon")]
        [SerializeField] protected Image icon;
        [SerializeField] protected Sprite iconOn;
        [SerializeField] protected Sprite iconOff;
        
        [Header("Background")]
        [SerializeField] protected Image graphic;
        
        private UnityAction onClickAction;

        private bool indicatorActive;
        private bool initialized;
        
        protected IUIService uiService;

#if DOTWEEN
        private Tweener buttonSelectTweener;
#else
        private Coroutine flipCoroutine;
#endif
        private (Vector3, Vector3) flipScale;

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        protected override void OnDestroy()
        {
            if (onClickAction != null)
                onClick.RemoveListener(onClickAction);
            
            base.OnDestroy();
        }

        protected virtual void Initialize()
        {
            if (initialized) return;
            
            uiService = Core.GetService<IUIService>();
            if (indicator) indicator.SetActive(indicatorActive);
            GetFlipScales();
            initialized = true;
        }

        public void SetLabel(string text)
        {
            if (!label)
            {
                Debug.Log($"{nameof(CustomButton)} on object {name}: label is not assigned");
                return;
            }
            label.SetText(text);
        }

        public void SetIndicatorActive(bool isActive, string labelText = "")
        {
            indicatorActive = isActive;
            
            indicatorLabel.SetText(labelText);
            indicatorLabel.enabled = !string.IsNullOrEmpty(labelText);
            
            indicator.SetActive(isActive);
        }

        public void SetIcon(Sprite sprite)
        {
            if (!icon) return;
            icon.sprite = sprite;
        }
        
#if DOTWEEN
        public void SetHighlight(bool isSelected, ButtonSelectType selectType, Sequence sequenceToJoin = null)
        {
            if (!icon) return;

            switch (selectType)
            {
                case ButtonSelectType.SpriteSwap:
                    icon.sprite = isSelected ? iconOn : iconOff;
                    break;
                case ButtonSelectType.Flip:
                    if (sequenceToJoin == null) buttonSelectTweener?.Kill();
                    var (toScale, fromScale) = isSelected ? flipScale : (flipScale.Item2, flipScale.Item1);

                    icon.transform.localScale = fromScale;
                    buttonSelectTweener = icon.transform.DOScale(toScale, 0.3f)
                        .SetEase(Ease.InOutQuad);
                    if (sequenceToJoin == null) buttonSelectTweener.Play();
                    else sequenceToJoin.Join(buttonSelectTweener);
                    break;
                case ButtonSelectType.TextColourSwap:
                    if (label) label.color = isSelected ? onColour : offColour;
                    break;
                case ButtonSelectType.IconColourSwap:
                    if (icon)  icon.color = isSelected ? onColour : offColour;
                    break;
                case ButtonSelectType.BackgroundColourSwap:
                    if (graphic)  graphic.color = isSelected ? onColour : offColour;
                    break;
                case ButtonSelectType.IconAndBackgroundColorSwap:
                    if (icon) icon.color = isSelected ? onColour : offColour;
                    if (graphic)  graphic.color = isSelected ? offColour : onColour;
                    break;
                case ButtonSelectType.IconAndBackgroundColorSwapReversed:
                    if (icon) icon.color = isSelected ? offColour : onColour;
                    if (graphic)  graphic.color = isSelected ? onColour : offColour;
                    break;
                default: break;
            }
        }
#else

        public void SetHighlight(bool isSelected, ButtonSelectType selectType)
        {
            if (!icon) return;

            switch (selectType)
            {
                case ButtonSelectType.SpriteSwap:
                    icon.sprite = isSelected ? iconOn : iconOff;
                    break;
                case ButtonSelectType.Flip:
                    if (flipCoroutine != null) StopCoroutine(flipCoroutine);
                    var (toScale, fromScale) = isSelected ? flipScale : (flipScale.Item2, flipScale.Item1);
                    flipCoroutine = StartCoroutine(AnimateScale(icon.transform, fromScale, toScale, 0.3f));
                    break;
                case ButtonSelectType.TextColourSwap:
                    if (label) label.color = isSelected ? onColour : offColour;
                    break;
                case ButtonSelectType.IconColourSwap:
                    if (icon) icon.color = isSelected ? onColour : offColour;
                    break;
                case ButtonSelectType.BackgroundColourSwap:
                    if (graphic) graphic.color = isSelected ? onColour : offColour;
                    break;
                case ButtonSelectType.IconAndBackgroundColorSwap:
                    if (icon) icon.color = isSelected ? onColour : offColour;
                    if (graphic) graphic.color = isSelected ? offColour : onColour;
                    break;
                case ButtonSelectType.IconAndBackgroundColorSwapReversed:
                    if (icon) icon.color = isSelected ? offColour : onColour;
                    if (graphic) graphic.color = isSelected ? onColour : offColour;
                    break;
                default: break;
            }
        }

        private System.Collections.IEnumerator AnimateScale(Transform target, Vector3 from, Vector3 to, float duration)
        {
            var time = 0f;
            target.localScale = from;
            while (time < duration)
            {
                var t = time / duration;
                t = Mathf.SmoothStep(0, 1, t);
                target.localScale = Vector3.Lerp(from, to, t);
                time += Time.deltaTime;
                yield return null;
            }
            target.localScale = to;
        }
#endif

        private void GetFlipScales()
        {
            flipScale = flipDirection switch
            {
                FlipDirection.Vertical => (Vector3.one, new Vector3(1f, -1f, 1f)),
                FlipDirection.VerticalReversed => (new Vector3(1f, -1f, 1f), Vector3.one),
                FlipDirection.Horizontal => (Vector3.one, new Vector3(-1f, 1f, 1f)),
                FlipDirection.HorizontalReversed => (new Vector3(-1f, 1f, 1f), Vector3.one),
                _ => (Vector3.one, Vector3.one)
            };
        }
        
        [UsedImplicitly]
        public void RequestBack()
        {
            uiService.RequestBack();
        }
    }
}