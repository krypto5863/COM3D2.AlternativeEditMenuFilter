using UnityEngine.Events;

namespace COM3D2.SimpleUI
{
    public interface ITextField : IContentControl, IStringControlValue
    {
        void AddSubmitCallback(UnityAction<string> callback);

        void RemoveSubmitCallback(UnityAction<string> callback);
    }
}