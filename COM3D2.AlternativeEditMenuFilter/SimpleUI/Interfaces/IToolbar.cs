using UnityEngine;

namespace COM3D2.SimpleUI
{
    public interface IToolbar : IControl, IIntControlValue
    {
        Color defaultColor { get; set; }
        Color selectedColor { get; set; }
        Color hoverColor { get; set; }
        Color disabledColor { get; set; }
    }
}