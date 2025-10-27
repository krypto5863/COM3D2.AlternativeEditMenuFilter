using COM3D2.SimpleUI;
using COM3D2.SimpleUI.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace COM3D2.AlternativeEditMenuFilter
{
    public class PresetPanelFilter : MonoBehaviour
    {
        private PresetPanelController controller;

        private string search = "";
        private bool updateItemListQueued = false;
        private bool showNames = false;

        private IButton caseSensitivityButton;
        private IButton termIncludeButton;
        private IButton showNamesButton;
        private ITextField searchTextField;
        private IDropdown historyDropdown;

        private readonly List<string> History = new List<string>();

        private PresetSearchConfig config;

        private bool SearchAllTerms
        {
            get => config.SearchAllTerms.Value;
            set => config.SearchAllTerms.Value = value;
        }

        private bool IgnoreCase
        {
            get => config.IgnoreCase.Value;
            set => config.IgnoreCase.Value = value;
        }

        private void Awake()
        {
        }

        public void Init(PresetSearchConfig config, Vector3 localPosition)
        {
            this.controller = new PresetPanelController(this.gameObject.transform.parent.gameObject);
            this.config = config;
            this.gameObject.transform.localPosition = localPosition;

            this.History.AddRange(
                this.config.History.Value
                .Split('\n')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s)));
        }

        private void Start()
        {
            Assert.IsNotNull(this.config, "Not properly initialized");
            this.BuildUI();
        }

        private void OnDisable()
        {
            Log.LogVerbose("Saving search history");
            this.config.History.Value = String.Join("\n", this.History.ToArray());
        }

        private void OnEnable()
        {
            this.QueueUpdateItemList();
        }

        private void Update()
        {
            if (updateItemListQueued)
            {
                updateItemListQueued = false;
                UpdateItemList();
            }
        }

        private void BuildUI()
        {
            var panelHeight = 40;

            var ui = SimpleUIRoot.Create(this.gameObject, 0, 0);

            var box = ui.Box(new Rect(0, 0, 1050, panelHeight + 16), "");

            var area = ui.Area(new Rect(8, 8, 1000, panelHeight), null);
            area.OnLayout(() =>
            {
                box.size = new Vector2(area.contentWidth + 16, area.contentHeight + 16);
            });

            historyDropdown = area.Dropdown(
                new Vector2(50, panelHeight),
                "Hist", null,
                this.History,
                this.OnHistoryDropdownChange);

            searchTextField = area.TextField(new Vector2(250, panelHeight), "");
            searchTextField.AddSubmitCallback(this.SearchTextfieldSubmit);

            area.Button(new Vector2(70, panelHeight), "Reset", this.ResetButtonClick);

            caseSensitivityButton = area.Button(new Vector2(50, panelHeight), "Aa", this.CaseSensitivityButtonClick);

            termIncludeButton = area.Button(new Vector2(50, panelHeight), "Or", this.TermIncludeButtonClick);

            showNamesButton = area.Button(new Vector2(50, panelHeight), "Name", this.ShowNamesButtonClick);
        }

        private void ShowNamesButtonClick()
        {
            showNames = !showNames;
            if (showNames)
            {
                controller.ShowLabels();
            }
            else
            {
                controller.HideLabels();
            }
            UpdateToggles();
        }

        private void OnHistoryDropdownChange(string selected)
        {
            searchTextField.Value = selected;
            SearchTextfieldSubmit(selected);
        }

        private void TermIncludeButtonClick()
        {
            this.SearchAllTerms = !this.SearchAllTerms;
            UpdateToggles();
            this.QueueUpdateItemList();
        }

        private void CaseSensitivityButtonClick()
        {
            this.IgnoreCase = !this.IgnoreCase;
            UpdateToggles();
            this.QueueUpdateItemList();
        }

        private void UpdateToggles()
        {
            if (this.SearchAllTerms)
            {
                this.termIncludeButton.text = "AND";
            }
            else
            {
                this.termIncludeButton.text = "OR";
            }

            if (this.IgnoreCase)
            {
                this.caseSensitivityButton.defaultColor = this.caseSensitivityButton.hoverColor = Color.white;
            }
            else
            {
                this.caseSensitivityButton.defaultColor = this.caseSensitivityButton.hoverColor = Color.gray;
            }

            if (this.showNames)
            {
                showNamesButton.defaultColor = showNamesButton.hoverColor = Color.white;
            }
            else
            {
                showNamesButton.defaultColor = showNamesButton.hoverColor = Color.gray;
            }
        }

        private void SearchTextfieldSubmit(string terms)
        {
            terms = terms.Trim();
            this.search = terms;
            if (terms == "")
            {
                this.ResetButtonClick();
                return;
            }
            AddToHistory(terms);
            this.QueueUpdateItemList();
        }

        private void AddToHistory(string terms)
        {
            var index = this.History.IndexOf(terms);
            if (index >= 0)
            {
                this.History.RemoveAt(index);
            }

            this.History.Insert(0, terms);

            var maxHistory = this.config.MaxHistory.Value;
            if (this.History.Count > maxHistory)
            {
                this.History.RemoveRange(maxHistory, this.History.Count - maxHistory);
            }

            this.historyDropdown.Choices = this.History;
        }

        private void QueueUpdateItemList()
        {
            updateItemListQueued = true;
        }

        private void UpdateItemList()
        {
            var termList = this.search
                .Split(' ')
                .Where(t => !string.IsNullOrEmpty(t))
                .Select(t => t.Trim())
                .ToArray();

            if (termList.Length == 0)
            {
                controller.ShowAll();
                controller.ResetView();
                return;
            }

            Log.LogVerbose("Performing filter");

            foreach (var item in controller.GetAllItems())
            {
                FilterItem(item, termList);
            }

            controller.ResetView();
        }

        private void FilterItem(PresetPanelItem item, string[] termList)
        {
            var inName = StringContains(item.Name, termList);
            if (!inName)
            {
                item.Visible = false;
                return;
            }

            item.Visible = true;
        }

        private bool StringContains(string str, string[] terms)
        {
            foreach (var term in terms)
            {
                if (StringContains(str, term))
                {
                    if (!SearchAllTerms)
                    {
                        return true;
                    }
                }
                else if (SearchAllTerms)
                {
                    return false;
                }
            }

            return SearchAllTerms;
        }

        private bool StringContains(string str, string term)
        {
            if (IgnoreCase)
            {
                CompareInfo compareInfo = CultureInfo.CurrentCulture.CompareInfo;
                int num = compareInfo.IndexOf(str, term, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);
                return (num >= 0);
            }

            return str.IndexOf(term) >= 0;
        }

        private void ResetButtonClick()
        {
            this.search = "";
            this.searchTextField.Value = "";
            controller.ShowAll();
            controller.ResetView();
        }
    }
}