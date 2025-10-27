namespace COM3D2.SimpleUI
{
    public interface ISlider : IControl, IFloatControlValue
    {
        void SetValues(float current, float minimum, float maximum);
    }
}