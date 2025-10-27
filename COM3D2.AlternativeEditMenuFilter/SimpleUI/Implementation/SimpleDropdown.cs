using COM3D2.SimpleUI.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.SimpleUI.Implementation
{
    public class SimpleDropdown : SimpleControl, IDropdown
    {
        protected bool ready = false;

        public string Value
        {
            get => uiPopupList.value;
            set => uiPopupList.value = value;
        }

        private IEnumerable<string> _choices;

        public IEnumerable<string> Choices
        {
            get => _choices;
            set
            {
                _choices = value;
                uiPopupList.Clear();
                foreach (var choice in value)
                {
                    uiPopupList.AddItem(choice);
                }
            }
        }

        protected UIPopupList uiPopupList;
        protected UISprite uiSprite;
        protected UIButton uiButton;
        protected UILabel uiLabel;

        private readonly TextChangeEvent onChange = new TextChangeEvent();

        public override void InitControl()
        {
            var atlas = UIUtils.GetAtlas("AtlasCommon");
            var spriteName = "cm3d2_common_plate_white";

            uiSprite = NGUITools.AddSprite(gameObject, atlas, spriteName);
            NGUITools.AddWidgetCollider(uiSprite.gameObject);
            uiPopupList = uiSprite.gameObject.AddComponent<UIPopupList>();
            uiPopupList.atlas = atlas;
            uiPopupList.backgroundSprite = "cm3d2_common_plate_white";
            uiPopupList.backgroundColor = Color.white;
            uiPopupList.highlightSprite = "cm3d2_common_plate_white";
            uiPopupList.highlightColor = new Color(.8f, .8f, .8f);
            uiPopupList.textColor = Color.black;
            uiPopupList.padding = new Vector2(4, 10);
            uiPopupList.trueTypeFont = UIUtils.GetFont("NotoSansCJKjp-DemiLight");
            uiPopupList.position = UIPopupList.Position.Below;
            uiPopupList.value = null;
            EventDelegate.Add(uiPopupList.onChange, new EventDelegate.Callback(uiPopupListChange));

            uiButton = uiSprite.gameObject.AddComponent<UIButton>();
            uiButton.hover = Color.white;
            uiButton.defaultColor = new Color(.9f, .9f, .9f);

            uiLabel = NGUITools.AddWidget<UILabel>(uiSprite.gameObject);
            uiLabel.trueTypeFont = UIUtils.GetFont("NotoSansCJKjp-DemiLight");
            uiLabel.color = Color.black;
        }

        private void Start()
        {
            // UIPopupList has an annoying feature of firing change event as soon as initialization
            // This delays forwarding change events untill that happens
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return null;
            ready = true;
        }

        public void AddChangeCallback(UnityAction<string> callback)
        {
            onChange.AddListener(callback);
        }

        public void RemoveChangeCallback(UnityAction<string> callback)
        {
            onChange.RemoveListener(callback);
        }

        protected virtual void uiPopupListChange()
        {
            if (ready)
            {
                onChange.Invoke(uiPopupList.value);
            }
        }

        public override void UpdateUI()
        {
            uiLabel.text = text;

            uiSprite.SetDimensions(
                Mathf.FloorToInt(size.x + .5f),
                Mathf.FloorToInt(size.y + .5f));
            uiSprite.ResizeCollider();
        }
    }
}