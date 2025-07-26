using IceCold.UI.Interface;

namespace IceCold.UI
{
    public class UI
    {
        private static IUIService service;
        
        public static void Init(IUIService uiService)
        {
            service = uiService;
        }

        public static T GetWindow<T>(string id = null) where T : class, IWindow
        {
            return service.GetWindow<T>(id);
        }

        public static T ShowWindow<T>(string id = null) where T : class, IWindow
        {
            var wnd = GetWindow<T>(id);
            wnd?.Show();
            return wnd;
        }

        public static T HideWindow<T>(string id = null) where T : class, IWindow
        {
            var wnd = GetWindow<T>(id);
            wnd?.Hide();
            return wnd;
        }

        public static void RequestBack(bool instant = false)
        {
            service.RequestBack(instant);
        }
    }
}