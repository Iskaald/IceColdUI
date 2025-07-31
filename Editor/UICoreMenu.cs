using IceCold.UI.Interface;
using UnityEditor;

namespace IceCold.Editor
{
    public class UICoreMenu : IceColdMenu
    {
        [MenuItem("IceCold/UI/Config", priority = 20)]
        private static void SelectUIConfig()
        {
            var config = FindConfigAsset<UIConfig>();
            
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
        }
        
        [MenuItem("IceCold/UI/Sprite Provider", priority = 21)]
        private static void SelectSpriteProviderConfig()
        {
            var config = FindConfigAsset<SpriteProviderConfig>();
            
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
        }
    }
}