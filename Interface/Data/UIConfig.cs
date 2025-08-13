using IceCold.Interface;
using UnityEngine;

namespace IceCold.UI.Interface
{
    [CreateAssetMenu(fileName = nameof(UIConfig), menuName = "IceCold/UI/Create Config", order = 0)]
    public class UIConfig : IceColdConfig
    {
        public override string Key => nameof(UIConfig);
        
        public int canvasSortingOrder = 10;
        public RenderMode canvasRenderMode = RenderMode.ScreenSpaceOverlay;
    }
}