using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.AlternativeEditMenuFilter
{
    public class EditMenuPanelController
    {
        private UIGrid m_grid;
        private UIPanel m_scrollViewPanel;
        private UIScrollView m_scrollView;
        private Transform m_gridTableTrans;
        private UIScrollBar m_scrollBar;
        private SceneEdit m_sceneEdit;
        //private readonly Dictionary<GameObject, EditMenuPanelItem> PanelItemCache = new Dictionary<GameObject, EditMenuPanelItem>();
        private readonly SimpleObjectPool<EditMenuPanelItem> PanelItemPool;

        public EditMenuPanelController(GameObject go)
        {
            m_scrollView = go.GetComponentInChildren<UIScrollView>(false);
            Assert.IsNotNull(m_scrollView, $"Could not find UIScrollView for {go}");

            m_scrollViewPanel = m_scrollView.GetComponent<UIPanel>();

            m_grid = m_scrollView.GetComponentInChildren<UIGrid>();
            Assert.IsNotNull(m_grid, $"Could not find UIGrid for {go}");

            m_gridTableTrans = m_grid.transform;

            m_sceneEdit = GameObject.Find("__SceneEdit__").GetComponent<SceneEdit>();
            Assert.IsNotNull(m_sceneEdit, $"Could not find SceneEdit");

            m_scrollBar = go.GetComponentInChildren<UIScrollBar>(false);
            Assert.IsNotNull(m_scrollBar, $"Could not find UIScrollBar for {go}");

            PanelItemPool = new SimpleObjectPool<EditMenuPanelItem>(null, item => item.Reset());
        }

        public IEnumerable<EditMenuPanelItem> GetAllItems()
        {
            var count = m_gridTableTrans.childCount;
            for (var i = 0; i < count; i++)
            {
                var item = m_gridTableTrans.GetChild(i);
                if (item == null) continue;

                var btn = item.Find("Button");
                if (btn == null) continue;

                var edit = btn.GetComponent<ButtonEdit>();
                if (edit == null || edit.m_MenuItem == null) continue;

                var menu = edit.m_MenuItem;
                if (string.IsNullOrEmpty(menu.m_strMenuFileName)) continue;
                if (menu.m_strMenuName == "無し") continue;
                if (menu.m_strMenuName.Contains("脱ぐ・外す")) continue;

                var editMenuPanelItem = PanelItemPool.Get();
                editMenuPanelItem.Initialize(edit.transform.parent.gameObject, menu);
                yield return editMenuPanelItem;
            }
        }

        public void ReleaseAllPooled()
        {
            PanelItemPool.ReleaseAll();
        }

        public void ResetView()
        {
            bool enabled = m_grid.enabled;
            m_grid.enabled = true;
            m_grid.hideInactive = true;
            m_grid.Reposition();
            m_scrollView.ResetPosition();
            m_scrollBar.value = 0f;
            m_grid.enabled = enabled;
            m_sceneEdit.HoverOutCallback();
        }

        public void ShowAll()
        {
            foreach (var r in GetAllItems())
            {
                if (!r.gameObject.activeSelf)
                {
                    r.gameObject.SetActive(true);
                }
            }

            ReleaseAllPooled();
        }

        internal void HidePanel()
        {
            m_scrollViewPanel.alpha = 0.0f;
        }

        internal void ShowPanel()
        {
            m_scrollViewPanel.alpha = 1.0f;
        }
    }
}