using UnityEngine;

using UnityEngine.Events;

namespace COM3D2.SimpleUI.Implementation
{
    public class SimpleButton : SimpleControl, IButton
    {
        private UISprite uiSprite;
        private UILabel uiLabel;
        private UIButton uiButton;

        private bool _isEnabled = true;

        public bool isEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = true;
                SetDirty();
            }
        }

        private readonly UnityEvent click = new UnityEvent();
        public UnityEvent Click => click;

        public Color defaultColor
        {
            get => uiButton.defaultColor;
            set => uiButton.defaultColor = value;
        }

        public Color hoverColor
        {
            get => uiButton.hover;
            set => uiButton.hover = value;
        }

        public Color disabledColor
        {
            get => uiButton.disabledColor;
            set => uiButton.disabledColor = value;
        }

        public Color activeColor
        {
            get => uiButton.pressed;
            set => uiButton.pressed = value;
        }

        public override void InitControl()
        {
            var atlas = UIUtils.GetAtlas("AtlasCommon");
            var spriteName = "cm3d2_common_plate_white";

            uiSprite = NGUITools.AddSprite(gameObject, atlas, spriteName);
            NGUITools.AddWidgetCollider(uiSprite.gameObject);

            uiButton = uiSprite.gameObject.AddComponent<UIButton>();
            uiButton.hover = Color.white;
            uiButton.defaultColor = new Color(.9f, .9f, .9f);
            EventDelegate.Add(uiButton.onClick, new EventDelegate.Callback(Click.Invoke));

            uiLabel = NGUITools.AddWidget<UILabel>(uiSprite.gameObject);
            uiLabel.trueTypeFont = UIUtils.GetFont("NotoSansCJKjp-DemiLight");
            uiLabel.color = Color.black;
        }

        public override void UpdateUI()
        {
            uiSprite.width = uiLabel.width = Mathf.FloorToInt(size.x + 0.5f);
            uiSprite.height = uiLabel.height = Mathf.FloorToInt(size.y + 0.5f);
            uiSprite.ResizeCollider();

            uiLabel.text = text;

            uiButton.isEnabled = isEnabled;
        }
    }
}