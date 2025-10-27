using UnityEngine;

namespace COM3D2.SimpleUI
{
    public interface IToggle : IContentControl, IBoolControlValue
    {
        Color defaultColor { get; set; }
        Color defaultActiveColor { get; set; }
        Color selectedColor { get; set; }
        Color selectedActiveColor { get; set; }
        Color disabledColor { get; set; }
    }
}