using UnityEngine;
using UnityEngine.UI;
#if DOTWEEN
using DG.Tweening;
#endif

namespace IceColdCore.UI.Interface
{
    public class WindowAnimationData
    {
        public Image fade;
        public GameObject window;
        public float animationDuration;

#if DOTWEEN
        public Ease animationEase;
#else
        public AnimationEase animationEase;
#endif

        public Color? defaultFadeColor;
        public Transform scalableContent;
        
        public WindowAnimationData(){}

#if DOTWEEN
        public WindowAnimationData(Image fadeImage, GameObject window, float animationDuration, Ease animationEase, Color? defaultFadeColor = null, Transform scalableContent = null)
#else
        public WindowAnimationData(Image fadeImage, GameObject window, float animationDuration, AnimationEase animationEase, Color? defaultFadeColor = null, Transform scalableContent = null)
#endif
        {
            fade = fadeImage;
            this.window = window;
            this.animationDuration = animationDuration;
#if DOTWEEN
            this.animationEase = animationEase;
#endif
            this.defaultFadeColor = defaultFadeColor;
            this.scalableContent = scalableContent;
        }
    }
}