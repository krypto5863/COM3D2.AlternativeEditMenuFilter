using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<GameObject, EditMenuPanelItem> PanelItemCache = new Dictionary<GameObject, EditMenuPanelItem>();

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
        }

        public IEnumerable<EditMenuPanelItem> GetAllItems()
        {
            return (from i in Enumerable.Range(0, m_gridTableTrans.childCount)
                    select m_gridTableTrans.GetChild(i) into item
                    where item != null
                    select item.Find("Button") into btn
                    where btn != null
                    select btn.GetComponent<ButtonEdit>() into edit
                    where edit != null && edit.m_MenuItem != null
                    where edit.m_MenuItem.m_strMenuFileName != ""
                    where edit.m_MenuItem.m_strMenuName != "無し"
                    where !edit.m_MenuItem.m_strMenuName.Contains("脱ぐ・外す")
                    select new EditMenuPanelItem(edit.transform.parent.gameObject, edit.m_MenuItem)
                    );
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