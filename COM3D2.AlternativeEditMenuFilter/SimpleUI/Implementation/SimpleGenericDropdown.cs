using System;
using System.Linq;
using UnityEngine.Events;

namespace COM3D2.SimpleUI.Implementation
{
    public class SimpleGenericDropdown : SimpleDropdown, IGenericDropdown
    {
        private readonly ChangeEvent changeEvent = new ChangeEvent();

        public virtual new object Value
        {
            get
            {
                if (uiPopupList.data is DropdownItem data)
                {
                    return data.Value;
                }
                return null;
            }
            set
            {
                var data = uiPopupList.itemData
                    .Select(o => o as DropdownItem)
                    .Where(o => o != null)
                    .Where(o => o.Value.Equals(value))
                    .FirstOrDefault();

                if (data != null)
                {
                    uiPopupList.value = data.ItemName;
                }
            }
        }

        public virtual DropdownItem ValueData => uiPopupList.data as DropdownItem;

        public virtual T GetValue<T>()
        {
            if (Value is T data)
            {
                return data;
            }
            return default(T);
        }

        public virtual IGenericDropdown SetValue<T>(T value)
        {
            Value = value;
            return this;
        }

        private bool updateTextOnValue = false;

        public bool UpdateTextOnValue
        {
            get => updateTextOnValue;
            set
            {
                updateTextOnValue = value;
                SetDirty();
            }
        }

        public override void InitControl()
        {
            base.InitControl();
            EventDelegate.Add(uiPopupList.onChange, new EventDelegate.Callback(uiPopupListDataChange));
        }

        protected virtual void uiPopupListDataChange()
        {
            if (ready)
            {
                if (UpdateTextOnValue)
                {
                    SetDirty();
                }

                changeEvent.Invoke(Value);
            }
        }

        public void AddChangeCallback(UnityAction<object> callback)
        {
            changeEvent.AddListener(callback);
        }

        public void RemoveChangeCallback(UnityAction<object> callback)
        {
            changeEvent.RemoveListener(callback);
        }

        public virtual IGenericDropdown ClearChoices()
        {
            uiPopupList.Clear();
            return this;
        }

        public IGenericDropdown Choice<T>(T value, string text = "", string selected = "")
        {
            if (string.IsNullOrEmpty(text)) text = value.ToString();
            if (string.IsNullOrEmpty(selected)) selected = text;

            uiPopupList.AddItem(text, new DropdownItem()
            {
                Value = value,
                ItemName = text,
                SelectedName = selected,
            });

            return this;
        }

        public IGenericDropdown RemoveChoice<T>(T value)
        {
            throw new NotImplementedException();
            return this;
        }

        public IGenericDropdown SetUpdateTextOnValuechange(bool value)
        {
            UpdateTextOnValue = value;
            return this;
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            var data = ValueData;
            if (UpdateTextOnValue && data != null)
            {
                uiLabel.text = data.SelectedName;
            }
        }

        public class DropdownItem
        {
            public object Value { get; set; }
            public string ItemName { get; set; }
            public string SelectedName { get; set; }
        }

        public class ChangeEvent : UnityEvent<object>
        {
        }
    }
}