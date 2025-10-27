using UnityEngine;

namespace COM3D2.SimpleUI.Implementation
{
    public class SimpleLabel : SimpleControl, ILabel
    {
        private UILabel uiLabel;

        public override void InitControl()
        {
            uiLabel = NGUITools.AddChild<UILabel>(gameObject);
            uiLabel.trueTypeFont = UIUtils.GetFont("NotoSansCJKjp-DemiLight");
            uiLabel.color = Color.white;
        }

        public override void UpdateUI()
        {
            uiLabel.SetDimensions(
                Mathf.FloorToInt(size.x + .5f),
                Mathf.FloorToInt(size.y + .5f));
            uiLabel.rawPivot = UIWidget.Pivot.Left;
            uiLabel.gameObject.transform.localPosition = new Vector3(-size.x / 2, 0);
            uiLabel.text = text;
        }
    }
}