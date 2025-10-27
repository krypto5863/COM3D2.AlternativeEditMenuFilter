using COM3D2.SimpleUI.Events;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.SimpleUI.Implementation
{
    public class SimpleTextField : SimpleControl, ITextField
    {
        private UISprite uiSprite;
        private UISprite bgSprite;
        private UIInput uiInput;
        private UILabel uiLabel;

        private readonly TextChangeEvent onSubmit = new TextChangeEvent();
        private readonly TextChangeEvent onChange = new TextChangeEvent();

        public string Value
        {
            get => uiInput.value;
            set => uiInput.value = value;
        }

        public override void InitControl()
        {
            var atlas = UIUtils.GetAtlas("AtlasCommon");
            bgSprite = NGUITools.AddSprite(gameObject, atlas, "cm3d2_common_plate_white");
            bgSprite.color = new Color(.2f, .2f, .2f);

            uiSprite = NGUITools.AddSprite(gameObject, atlas, "cm3d2_common_lineframe_white");
            uiSprite.color = Color.gray;
            NGUITools.AddWidgetCollider(uiSprite.gameObject);

            uiLabel = NGUITools.AddWidget<UILabel>(uiSprite.gameObject);
            uiLabel.trueTypeFont = UIUtils.GetFont("NotoSansCJKjp-DemiLight");
            uiLabel.rawPivot = UIWidget.Pivot.Left;

            uiInput = uiSprite.gameObject.AddComponent<UIInput>();
            uiInput.label = uiLabel;
            uiInput.value = "";
            uiInput.activeTextColor = Color.white;
            uiInput.caretColor = Color.gray;
            uiInput.onReturnKey = UIInput.OnReturnKey.Submit;

            EventDelegate.Add(uiInput.onChange, new EventDelegate.Callback(ChangeEvent));
            EventDelegate.Add(uiInput.onSubmit, new EventDelegate.Callback(SubmitEvent));
        }

        private void ChangeEvent()
        {
            onChange.Invoke(Value);
        }

        private void SubmitEvent()
        {
            onSubmit.Invoke(Value);
        }

        public override void UpdateUI()
        {
            var width = Mathf.FloorToInt(size.x + 0.5f);
            var height = Mathf.FloorToInt(size.y + 0.5f);

            bgSprite.SetDimensions(width, height);
            uiSprite.SetDimensions(width, height);
            uiLabel.SetDimensions(width - 20, height - 20);
            uiLabel.gameObject.transform.localPosition = new Vector3(-size.x / 2f + 10, 0);

            uiSprite.ResizeCollider();
        }

        public void AddChangeCallback(UnityAction<string> callback)
        {
            onChange.AddListener(callback);
        }

        public void RemoveChangeCallback(UnityAction<string> callback)
        {
            onChange.RemoveListener(callback);
        }

        public void AddSubmitCallback(UnityAction<string> callback)
        {
            onSubmit.AddListener(callback);
        }

        public void RemoveSubmitCallback(UnityAction<string> callback)
        {
            onSubmit.RemoveListener(callback);
        }
    }
}