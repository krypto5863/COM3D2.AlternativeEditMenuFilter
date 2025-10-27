using COM3D2.SimpleUI.Events;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.SimpleUI.Implementation
{
    public class SimpleToggle : SimpleControl, IToggle
    {
        private BoxCollider boxCollider;
        private UISprite checkSprite;
        private UISprite boxSprite;
        private UILabel uiLabel;
        private UIButton uiButton;

        private bool _value;

        public bool Value
        {
            get => _value;
            set
            {
                _value = value;
                checkSprite.gameObject.SetActive(value);
            }
        }

        public Color defaultColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Color defaultActiveColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Color selectedColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Color selectedActiveColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Color disabledColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private readonly ToggleEvent onSelected = new ToggleEvent();
        public ToggleEvent OnSelected => onSelected;

        public override void InitControl()
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();

            var atlas = UIUtils.GetAtlas("AtlasCommon");

            boxSprite = NGUITools.AddSprite(gameObject, atlas, "cm3d2_common_lineframe_white");
            boxSprite.SetDimensions(20, 20);

            checkSprite = NGUITools.AddSprite(boxSprite.gameObject, atlas, "cm3d2_common_plate_white");
            checkSprite.SetDimensions(14, 14);

            uiLabel = NGUITools.AddWidget<UILabel>(gameObject);
            uiLabel.trueTypeFont = UIUtils.GetFont("NotoSansCJKjp-DemiLight");
            uiLabel.color = Color.white;
            uiLabel.rawPivot = UIWidget.Pivot.Left;

            Value = false;

            uiButton = gameObject.AddComponent<UIButton>();
            uiButton.tweenTarget = boxSprite.gameObject;
            uiButton.hover = Color.white;
            uiButton.defaultColor = new Color(1, 1, 1, 0.9f);
            EventDelegate.Add(uiButton.onClick, new EventDelegate.Callback(delegate ()
            {
                Value = !Value;
                OnSelected.Invoke(Value);
            }));
        }

        public override void UpdateUI()
        {
            boxCollider.size = size;
            boxSprite.gameObject.transform.localPosition = new Vector3(-size.x / 2 + 10, 0);

            var labelWidth = size.x - 30;

            uiLabel.SetDimensions(Mathf.FloorToInt(labelWidth + 0.5f), Mathf.FloorToInt(size.y + 0.5f));
            uiLabel.gameObject.transform.localPosition = new Vector3(
                -labelWidth / 2 + 15, 0);
            uiLabel.text = text;
        }

        public void AddChangeCallback(UnityAction<bool> callback)
        {
            onSelected.AddListener(callback);
        }

        public void RemoveChangeCallback(UnityAction<bool> callback)
        {
            onSelected.RemoveListener(callback);
        }
    }
}