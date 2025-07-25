#if DOTWEEN
using DG.Tweening;
#endif
using IceColdCore.UI.Interface;
using UnityEngine;
using UnityEngine.UI;
using Action = System.Action;

namespace IceColdCore.UI
{
    public abstract class BaseWindow<TView> : MonoBehaviour, IWindow<TView>, IInternalWindow where TView : IWindowView
    {
        public event Action WillShow;
        public event Action Shown;
        public event Action WillHide;
        public event Action Hidden;

        [SerializeField] private bool startHidden;

        [Header("Window Identifier")]
        [SerializeField] private string id;

        [SerializeField] private WindowType windowType;

        [Header("Window Animation")]
        [SerializeField] private bool animated;
#if DOTWEEN
        [SerializeField] private Ease animationEase = Ease.InQuad;
        #else
        [SerializeField] private AnimationEase animationEase = AnimationEase.EaseInOut;
#endif
        [SerializeField] private float animationDuration = 0.25f;
        [SerializeField] private Image fade;
        [SerializeField] private Transform scalableContent;

        [SerializeField] private TView view;

        private Color? defaultFadeColor;
        private WindowAnimator animator;
        private bool isInitializing;

        private IUIService uiService;

        public TView View => view;
        public abstract bool IsInitialized { get; }
        public string Id => id;

        #region Abstract Methods
        protected abstract void Initialize();
        protected abstract void UpdateData();
        #endregion

        public void ReadyWindow()
        {
            if (isInitializing) return;
            isInitializing = true;
            try
            {
                uiService = Core.GetService<IUIService>();
#if DOTWEEN
                animator = new WindowAnimator();
#else
                if (!TryGetComponent(out animator))
                    animator = gameObject.AddComponent<WindowAnimator>();
#endif

                if (startHidden) HideInstant();
                else ShowInstant();

                Initialize();
            }
            finally
            {
                isInitializing = false;
            }
        }

        public void ShowInternal()
        {
            WillShow?.Invoke();

            if (!animated)
            {
                ShowInstant();
                return;
            }

            ProcessShow();
            AnimateShow();
        }

        public void ShowInstantInternal()
        {
            ProcessShow();

            if (scalableContent)
                scalableContent.localScale = Vector3.one;
            transform.localScale = Vector3.one;

            gameObject.SetActive(true);
            OnShown();
        }

        public void HideInternal()
        {
            WillHide?.Invoke();

            if (!animated)
            {
                HideInstant();
                return;
            }

            AnimateHide();
        }

        public void HideInstantInternal()
        {
            OnHidden();
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }

       public void Reparent(Transform parent, bool worldPositionStays)
        {
            transform.SetParent(parent, worldPositionStays);
        }

        public void Show()
        {
            if (windowType != WindowType.Standard) ((IInternalWindow)this).ShowInternal();
            else uiService.ShowWindow(this);
        }

        public void ShowInstant()
        {
            if (windowType != WindowType.Standard) ((IInternalWindow)this).ShowInstantInternal();
            else uiService.ShowWindow(this, true);
        }

        public void Hide()
        {
            if (windowType != WindowType.Standard) ((IInternalWindow)this).HideInternal();
            else uiService.HideWindow(this);
        }

        public void HideInstant()
        {
            if (windowType != WindowType.Standard) ((IInternalWindow)this).HideInstantInternal();
            else uiService.HideWindow(this, true);
        }

        private void OnWillShow()
        {
            WillShow?.Invoke();
        }

        private void OnWillHide()
        {
            WillHide?.Invoke();
        }

        protected virtual void OnShown()
        {
            Shown?.Invoke();
        }

        protected virtual void OnHidden()
        {
            gameObject.SetActive(false);
            Hidden?.Invoke();
        }

        private void ProcessShow()
        {
            UpdateData();
        }

        private void AnimateShow()
        {
            StoreSettings();

            var doTweenAnimationData = new WindowAnimationData(fade, gameObject, animationDuration, animationEase, defaultFadeColor, scalableContent);
            animator.AnimateShow(doTweenAnimationData, OnShown);
        }

        private void AnimateHide()
        {
            StoreSettings();
            
            var doTweenAnimationData = new WindowAnimationData(fade, gameObject, animationDuration, animationEase, defaultFadeColor, scalableContent);
            animator.AnimateHide(doTweenAnimationData, OnHidden);
        }

        private void StoreSettings()
        {
            if (!fade || defaultFadeColor != null && defaultFadeColor.HasValue) return;

            defaultFadeColor = fade.color;
        }
    }
}