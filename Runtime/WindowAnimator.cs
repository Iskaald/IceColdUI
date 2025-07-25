using System;
using IceColdCore.UI.Interface;
using UnityEngine;
#if DOTWEEN
using DG.Tweening;
using UnityEngine;
#else
using System.Collections;
#endif

namespace IceColdCore.UI
{
#if DOTWEEN
    public class WindowAnimator
#else
    public class WindowAnimator : MonoBehaviour
#endif
    {
#if DOTWEEN
        private Sequence animationSequence;
#else
        private Coroutine animationCoroutine;
#endif

        public void AnimateShow(WindowAnimationData data, Action callback = null)
        {
#if DOTWEEN
            AnimateShowWithDoTween(data, callback);
#else
            ProcessAnimateShow(data, callback);
#endif
        }

        public void AnimateHide(WindowAnimationData data, Action callback = null)
        {
#if DOTWEEN
            AnimateHideWithDoTween(data,callback);
#else
            ProcessAnimateHide(data, callback);
#endif
        }

#if DOTWEEN
        private void AnimateShowWithDoTween(WindowAnimationData data, Action callback)
        {
            animationSequence?.Complete(false);
            
            if (data.fade != null)
            {
                var transparent = data.fade.color;
                transparent.a = 0;
                data.fade.color = transparent;
            }

            if (data.scalableContent == null) data.scalableContent = data.window.transform;
            data.scalableContent.localScale = Vector3.zero;
            data.window.SetActive(true);
            
            animationSequence = DOTween.Sequence();
            animationSequence.OnComplete(() => callback?.Invoke());
            
            animationSequence.Append(data.scalableContent.DOScale(Vector3.one, data.animationDuration).SetEase(data.animationEase));
            if (data.fade != null)
                animationSequence.Join(data.fade.DOColor(data.defaultFadeColor!.Value, data.animationDuration).SetEase(data.animationEase));

            animationSequence.Play();
        }
        
        private void AnimateHideWithDoTween(WindowAnimationData data, Action callback)
        {
            animationSequence?.Complete(false);
            
            animationSequence = DOTween.Sequence();
            animationSequence.OnComplete(() => callback?.Invoke());

            var transparent = Color.white;
            if (data.defaultFadeColor != null)
            {
                transparent = data.defaultFadeColor!.Value;
                transparent.a = 0;
            }
            
            if (data.scalableContent == null) data.scalableContent = data.window.transform;
            
            animationSequence.Append(data.scalableContent.DOScale(Vector3.zero, data.animationDuration).SetEase(data.animationEase));
            if (data.fade != null)
                animationSequence.Append(data.fade.DOColor(transparent, data.animationDuration).SetEase(data.animationEase));
            
            animationSequence.Play();
        }
#else

        private void ProcessAnimateShow(WindowAnimationData data, Action callback)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            
            if (data.scalableContent == null)
            {
                data.scalableContent = data.window.transform;
            }

            var startColor = Color.clear;
            var endColor = data.defaultFadeColor ?? Color.white;
            if (data.fade != null)
            {
                startColor = endColor;
                startColor.a = 0;
                data.fade.color = startColor;
            }

            data.scalableContent.localScale = Vector3.zero;

            data.window.SetActive(true);

            animationCoroutine = StartCoroutine(AnimateShowCoroutine(data, startColor, endColor, callback));
        }

        private void ProcessAnimateHide(WindowAnimationData data, Action callback)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            if (data.scalableContent == null)
            {
                data.scalableContent = data.window.transform;
            }

            var startColor = data.defaultFadeColor ?? Color.white;
            var endColor = startColor;
            endColor.a = 0;

            animationCoroutine = StartCoroutine(AnimateHideCoroutine(data, startColor, endColor, callback));
        }

        private static float GetEasedValue(float t, AnimationEase ease)
        {
            switch (ease)
            {
                case AnimationEase.EaseIn:
                    return t * t;
                case AnimationEase.EaseOut:
                    return 1 - (1 - t) * (1 - t);
                case AnimationEase.EaseInOut:
                    return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
                case AnimationEase.Linear:
                default:
                    return t;
            }
        }

        private IEnumerator AnimateShowCoroutine(WindowAnimationData data, Color startColor, Color endColor, Action callback)
        {
            var elapsedTime = 0f;
            while (elapsedTime < data.animationDuration)
            {
                var progress = elapsedTime / data.animationDuration;

                var easedProgress = GetEasedValue(progress, data.animationEase);

                data.scalableContent.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, easedProgress);

                if (data.fade != null)
                {
                    data.fade.color = Color.LerpUnclamped(startColor, endColor, easedProgress);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            data.scalableContent.localScale = Vector3.one;
            if (data.fade != null)
            {
                data.fade.color = endColor;
            }

            animationCoroutine = null;

            callback?.Invoke();
        }
        
        private IEnumerator AnimateHideCoroutine(WindowAnimationData data, Color startColor, Color endColor, Action callback)
        {
            var elapsedTime = 0f;
            while (elapsedTime < data.animationDuration)
            {
                var progress = elapsedTime / data.animationDuration;
                var easedProgress = GetEasedValue(progress, data.animationEase);

                data.scalableContent.localScale = Vector3.LerpUnclamped(Vector3.one, Vector3.zero, easedProgress);

                if (data.fade != null)
                {
                    data.fade.color = Color.LerpUnclamped(startColor, endColor, easedProgress);
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            data.scalableContent.localScale = Vector3.zero;
            if (data.fade != null)
            {
                data.fade.color = endColor;
            }

            animationCoroutine = null;
            
            callback?.Invoke();
            data.window.SetActive(false);
        }
#endif
    }
}