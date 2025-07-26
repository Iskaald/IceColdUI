using UnityEngine;

namespace IceCold.UI.Interface
{
    public interface IInternalWindow
    {
        public void ReadyWindow();
        public void ShowInternal();
        public void ShowInstantInternal();
        public void HideInternal();
        public void HideInstantInternal();
        public void DestroySelf();
        public void Reparent(Transform parent, bool worldPositionStays);
    }
}