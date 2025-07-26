using IceCold.Interface;

namespace IceCold.UI.Interface
{
    public interface IUIService : ICoreService
    {
        public T GetWindow<T>(string id = null) where T : class, IWindow;
        public IWindow GetWindow(string id);
        public void ShowWindow(string id, bool instant = false);
        public void ShowWindow(IWindow wnd, bool instant = false);
        public void HideWindow(string id, bool instant = false);
        public void HideWindow(IWindow wnd, bool instant = false);
        public void RequestBack(bool instant = false);
    }
}