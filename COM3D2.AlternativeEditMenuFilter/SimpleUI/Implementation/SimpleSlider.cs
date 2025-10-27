using COM3D2.SimpleUI.Events;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.SimpleUI.Implementation
{
    public class SimpleSlider : SimpleControl, ISlider
    {
        private UISprite uiBackground;
        private UIButton uiBackgroundBtn;
        private UIWidget uiForeground;
        private UISprite uiThumb;
        private UIButton uiThumbBtn;
        private UISlider uiSlider;

        private BoxCollider uiBackgroundCollider;
        private BoxCollider uiThumbCollider;

        private readonly SliderEvent onChange = new SliderEvent();

        public enum SliderDirection
        {
            HORIZONTAL,
            VERTICAL
        }

        private SliderDirection _direction;

        public SliderDirection direction
        {
            get => _direction;
            set
            {
                _direction = value;
                SetDirty();
            }
        }

        private float _minValue = 0f;
        private float _maxValue = 1f;

        public float Value
        {
            get
            {
                var range = _maxValue - _minValue;
                return _minValue + uiSlider.value * range;
            }
            set
            {
                if (value > _maxValue || value < _minValue)
                {
                    throw new Exception($"Invalid value {value}");
                }

                var offset = value - _minValue;
                uiSlider.value = offset / (_maxValue - _minValue);
            }
        }

        private int _thickness = 10;

        public int thickness
        {
            get => _thickness;
            set
            {
                _thickness = value;
                SetDirty();
            }
        }

        public override void InitControl()
        {
            var atlas = UIUtils.GetAtlas("AtlasCommon");
            uiBackground = NGUITools.AddSprite(gameObject, atlas, "cm3d2_common_plate_white");
            var objBackground = uiBackground.gameObject;
            uiBackgroundCollider = objBackground.AddComponent<BoxCollider>();
            uiBackgroundBtn = objBackground.AddComponent<UIButton>();
            uiBackground.ResizeCollider();

            uiForeground = NGUITools.AddWidget<UIWidget>(objBackground);

            uiThumb = NGUITools.AddSprite(objBackground, atlas, "cm3d2_common_plate_white");
            var objThumb = uiThumb.gameObject;
            uiThumbCollider = objThumb.AddComponent<BoxCollider>();
            uiThumbBtn = objThumb.AddComponent<UIButton>();
            uiThumb.ResizeCollider();

            uiSlider = objBackground.AddComponent<UISlider>();
            uiSlider.backgroundWidget = uiBackground;
            uiSlider.foregroundWidget = uiForeground;
            uiSlider.thumb = objThumb.transform;

            EventDelegate.Add(uiSlider.onChange, new EventDelegate.Callback(this.OnSliderChange));
        }

        private void OnSliderChange()
        {
            onChange.Invoke(this.Value);
        }

        public override void UpdateUI()
        {
            int controlWidth;
            int controlHeight;

            if (this.direction == SliderDirection.HORIZONTAL)
            {
                controlWidth = Mathf.FloorToInt(this.size.x + 0.5f) - 10;
                controlHeight = this.thickness;
            }
            else
            {
                controlWidth = this.thickness;
                controlHeight = Mathf.FloorToInt(this.size.y + 0.5f) - 10;
            }

            uiForeground.SetDimensions(controlWidth, controlHeight);

            uiBackground.SetDimensions(controlWidth, controlHeight);
            uiBackgroundCollider.size = this.size;
            uiBackgroundBtn.defaultColor = new Color(.5f, .5f, .5f);
            uiBackgroundBtn.hover = new Color(.8f, .8f, .8f);
            uiBackgroundBtn.pressed = new Color(.8f, .8f, .8f);

            uiThumb.SetDimensions(this.thickness + 6, this.thickness + 6);
            uiThumbCollider.size = new Vector2(this.thickness + 6, this.size.y);
            uiThumbBtn.defaultColor = new Color(.8f, .8f, .8f);
            uiThumbBtn.hover = Color.white;
            uiThumbBtn.pressed = Color.white;

            uiSlider.fillDirection = this.direction == SliderDirection.HORIZONTAL ? UIProgressBar.FillDirection.LeftToRight : UIProgressBar.FillDirection.BottomToTop;
            uiSlider.ForceUpdate();
        }

        public void SetValues(float current, float minimum, float maximum)
        {
            _minValue = minimum;
            _maxValue = maximum;
            this.Value = current;
        }

        public void AddChangeCallback(UnityAction<float> callback)
        {
            onChange.AddListener(callback);
        }

        public void RemoveChangeCallback(UnityAction<float> callback)
        {
            onChange.AddListener(callback);
        }
    }
}