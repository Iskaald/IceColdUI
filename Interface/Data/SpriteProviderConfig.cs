using System.Collections.Generic;
using IceCold.Interface;
using UnityEngine;

namespace IceCold.UI.Interface
{
    [CreateAssetMenu(fileName = nameof(SpriteProviderConfig), menuName = "IceCold/UI/Create Sprite Provider", order = 0)]
    public class SpriteProviderConfig : IceColdConfig
    {
        public override string Key => nameof(SpriteProviderConfig);
        
        public Sprite falloutSprite = null;
        public List<IconData> sprites = new();
        public List<ColorData> colors = new();
    }
}