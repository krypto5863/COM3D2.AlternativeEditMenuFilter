using UnityEngine;

namespace COM3D2.SimpleUI.Implementation
{
    public class SimpleBox : SimpleControl, IBox
    {
        private UISprite uiSprite;
        private UILabel uiLabel;

        public Color textColor
        {
            get => uiLabel.color;
            set => uiLabel.color = value;
        }

        public override void UpdateUI()
        {
            var width = Mathf.FloorToInt(size.x + 0.5f);
            var height = Mathf.FloorToInt(size.y + 0.5f);
            uiSprite.SetDimensions(width, height);
            uiLabel.SetDimensions(width, height);
            uiLabel.gameObject.transform.localPosition = new Vector3(0, size.y / 2f - 10);

            uiLabel.text = text;
        }

        public override void InitControl()
        {
            var atlas = UIUtils.GetAtlas("AtlasCommon");
            var spriteName = "cm3d2_common_plate_black";

            uiSprite = NGUITools.AddSprite(gameObject, atlas, spriteName);
            uiLabel = NGUITools.AddWidget<UILabel>(uiSprite.gameObject);

            uiLabel.trueTypeFont = UIUtils.GetFont("NotoSansCJKjp-DemiLight");
            uiLabel.color = Color.white;
            uiLabel.rawPivot = UIWidget.Pivot.Top;
        }

        public void ChangeSprite(UIAtlas atlas, string spriteName)
        {
            uiSprite.atlas = atlas;
            uiSprite.spriteName = spriteName;
        }
    }
}