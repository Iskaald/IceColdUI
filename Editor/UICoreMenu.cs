using IceColdCore.UI.Interface;
using UnityEditor;

namespace IceColdCore.Editor
{
    public class UICoreMenu : CoreMenu
    {
        [MenuItem("Core/UI/Config", priority = 20)]
        private static void SelectUIConfig()
        {
            var config = FindConfigAsset<UIConfig>();
            
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
        }
        
        [MenuItem("Core/UI/Sprite Provider", priority = 21)]
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