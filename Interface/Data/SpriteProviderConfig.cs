using System.Collections.Generic;
using IceColdCore.Interface;
using UnityEngine;

namespace IceColdCore.UI.Interface
{
    [CreateAssetMenu(fileName = nameof(SpriteProviderConfig), menuName = "Core/UI/Create Sprite Provider", order = 0)]
    public class SpriteProviderConfig : CoreConfig
    {
        public static string ConfigKey => nameof(SpriteProviderConfig);
        public override string Key => ConfigKey;
        
        public Sprite falloutSprite = null;
        public List<IconData> sprites = new();
        public List<ColorData> colors = new();
    }
}