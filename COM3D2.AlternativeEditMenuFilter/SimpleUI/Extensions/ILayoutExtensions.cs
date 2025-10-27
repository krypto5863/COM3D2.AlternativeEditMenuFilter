using UnityEngine.Events;

namespace COM3D2.SimpleUI.Extensions
{
    public static class ILayoutExtensions
    {
        public static T OnLayout<T>(this T layout, UnityAction callback) where T : ILayout
        {
            layout.AddLayoutCallback(callback);
            return layout;
        }
    }
}