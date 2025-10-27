using UnityEngine;

namespace COM3D2.SimpleUI.Implementation
{
    public abstract class SimpleControl : MonoBehaviour, IControl, ILayoutComponent
    {
        private Vector2 _size;
        private Vector2 _position;
        private string _text;
        private UITexture _texture;
        private bool _dirty = true;

        private BaseLayout _parent;

        public Vector2 size
        {
            get => _size;
            set => SetSize(value, true);
        }

        public Vector2 position
        {
            get => _position;
            set => SetPosition(value, true);
        }

        public string text
        {
            get => _text;
            set
            {
                _text = value;
                SetDirty();
            }
        }

        public UITexture texture
        {
            get => _texture;
            set
            {
                _texture = value;
                SetDirty();
            }
        }

        public string Name
        {
            get => gameObject.name;
            set => gameObject.name = value;
        }

        public string tooltip { get; set; }

        public void SetSize(Vector2 size, bool triggerLayout)
        {
            _size = size;
            SetDirty();
            if (triggerLayout)
            {
                _parent.SetDirty();
            }
        }

        public void SetPosition(Vector2 position, bool triggerLayout)
        {
            _position = position;
            if (triggerLayout)
            {
                _parent.SetDirty();
            }
        }

        public void Init(BaseLayout parent)
        {
            gameObject.name = GetType().Name;
            _parent = parent;
            InitControl();
        }

        public void SetDirty()
        {
            _dirty = true;
        }

        public void Update()
        {
            if (_dirty)
            {
                _dirty = false;
                UpdateUI();
            }
        }

        public abstract void UpdateUI();

        public abstract void InitControl();

        public void Remove()
        {
            if (_parent)
            {
                _parent.Remove(this);
                _parent = null;
            }
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_parent)
            {
                _parent.Remove(this);
            }
        }

        public virtual bool Visible
        {
            get => gameObject.activeSelf;
            set
            {
                gameObject.SetActive(value);
                _parent.SetDirty();
            }
        }
    }
}