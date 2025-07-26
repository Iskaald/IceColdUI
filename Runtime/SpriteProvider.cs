using System.Linq;
using IceCold.UI.Interface;
using UnityEngine;

namespace IceCold.UI
{
    public static class SpriteProvider
    {
        private static bool isInitialized;

        private static SpriteProviderConfig data;

        public static void Initialize()
        {
            if (isInitialized) return;

            data = Resources.Load<SpriteProviderConfig>(SpriteProviderConfig.ConfigKey);
            isInitialized = true;
        }

        public static Sprite GetSprite(string id)
        {
            if (!isInitialized) Initialize();
            
            var obj = data.sprites.FirstOrDefault(x => x.id == id);
            return obj == null ? data.falloutSprite : obj.sprite;
        }
        
        public static Sprite GetSprite(string id, string falloutSpriteId)
        {
            if (!isInitialized) Initialize();
            
            var obj = data.sprites.FirstOrDefault(x => x.id == id);
            var fallout = data.sprites.FirstOrDefault(x => x.id == falloutSpriteId);
            var falloutSprite = fallout?.sprite;
            return obj == null ? falloutSprite : obj.sprite;
        }
        
        public static Sprite GetSprite(string id, Sprite falloutSprite)
        {
            if (!isInitialized) Initialize();
            
            var obj = data.sprites.FirstOrDefault(x => x.id == id);
            return obj == null ? falloutSprite : obj.sprite;
        }

        public static Color GetColor(string rewardType)
        {
            if (!isInitialized) Initialize();
            
            var obj = data.colors.FirstOrDefault(x => x.id == rewardType);
            return obj?.color ?? Color.white;
        }
    }
}