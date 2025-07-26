namespace IceCold.UI.Interface
{
    public interface IWindowManager
    {
        public T GetWindow<T>(string id = null) where T : class, IWindow;
    }
}