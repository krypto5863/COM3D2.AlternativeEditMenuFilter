using COM3D2.SimpleUI.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace COM3D2.SimpleUI.Implementation
{
    public class SimpleToolbar : SimpleControl, IToolbar
    {
        private SimpleAutoLayout buttonLayout;
        private readonly List<SimpleButton> buttonList = new List<SimpleButton>();

        private int _value;

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                this.SetDirty();
            }
        }

        private string[] _choices;

        public string[] Choices
        {
            get => this._choices;
            set
            {
                this._choices = value;
                this.UpdateChoiceButtons();
                this.SetDirty();
            }
        }

        private Color _defaultColor = new Color(.4f, .4f, .4f);

        public Color defaultColor
        {
            get => _defaultColor;
            set
            {
                _defaultColor = value;
                SetDirty();
            }
        }

        private Color _selectedColor = new Color(.8f, .8f, .8f);

        public Color selectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                SetDirty();
            }
        }

        private Color _hoverColor = Color.white;

        public Color hoverColor
        {
            get => _hoverColor;
            set
            {
                _hoverColor = value;
                SetDirty();
            }
        }

        private Color _disabledColor = new Color(.1f, .1f, .1f);

        public Color disabledColor
        {
            get => _disabledColor;
            set
            {
                _disabledColor = value;
                SetDirty();
            }
        }

        private readonly ToolbarSelectedEvent onSelect = new ToolbarSelectedEvent();

        public override void InitControl()
        {
            buttonLayout = gameObject.AddComponent<SimpleAutoLayout>();
            buttonLayout.SetSize(this.size, false);
        }

        public void UpdateChoiceButtons()
        {
            foreach (var btn in this.buttonList)
            {
                Destroy(btn.gameObject);
            }

            this.buttonList.Clear();

            for (var i = 0; i < Choices.Length; i++)
            {
                var choice = Choices[i];
                var choiceI = i; // snapshot i
                var btn = buttonLayout.Button(new Vector2(20, this.size.y), choice, delegate ()
                {
                    this.Value = choiceI;
                    this.SetDirty();
                    this.onSelect.Invoke(this.Value);
                });

                this.buttonList.Add((SimpleButton)btn);
            }

            SetDirty();
        }

        public override void UpdateUI()
        {
            var numButtons = this.Choices.Length;
            var totalWidthWithoutSpacing = this.size.x - (this.buttonLayout.spacing * (numButtons - 1));
            var buttonWidth = totalWidthWithoutSpacing / numButtons;

            for (var i = 0; i < buttonList.Count; i++)
            {
                var button = buttonList[i];
                button.activeColor = this.hoverColor;
                button.SetSize(new Vector2(buttonWidth, this.size.y), false);
                if (i == this.Value)
                {
                    button.defaultColor = this.selectedColor;
                }
                else
                {
                    button.defaultColor = this.defaultColor;
                }
            }

            buttonLayout.SetSize(this.size, false);
            buttonLayout.SetDirty();
        }

        public void AddChangeCallback(UnityAction<int> callback)
        {
            this.onSelect.AddListener(callback);
        }

        public void RemoveChangeCallback(UnityAction<int> callback)
        {
            this.onSelect.RemoveListener(callback);
        }
    }
}