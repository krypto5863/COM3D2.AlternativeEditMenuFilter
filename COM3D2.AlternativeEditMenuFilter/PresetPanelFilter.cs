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
            controller = new PresetPanelController(gameObject.transform.parent.gameObject);
            this.config = config;
            gameObject.transform.localPosition = localPosition;

            History.AddRange(
                this.config.History.Value
                .Split('\n')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s)));
        }

        private void Start()
        {
            Assert.IsNotNull(config, "Not properly initialized");
            BuildUI();
        }

        private void OnDisable()
        {
            Log.LogVerbose("Saving search history");
            config.History.Value = String.Join("\n", History.ToArray());
        }

        private void OnEnable()
        {
            QueueUpdateItemList();
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

            var ui = SimpleUIRoot.Create(gameObject, 0, 0);

            var box = ui.Box(new Rect(0, 0, 1050, panelHeight + 16), "");

            var area = ui.Area(new Rect(8, 8, 1000, panelHeight), null);
            area.OnLayout(() =>
            {
                box.size = new Vector2(area.contentWidth + 16, area.contentHeight + 16);
            });

            historyDropdown = area.Dropdown(
                new Vector2(50, panelHeight),
                "Hist", null,
                History,
                OnHistoryDropdownChange);

            searchTextField = area.TextField(new Vector2(250, panelHeight), "");
            searchTextField.AddSubmitCallback(SearchTextfieldSubmit);

            area.Button(new Vector2(70, panelHeight), "Reset", ResetButtonClick);

            caseSensitivityButton = area.Button(new Vector2(50, panelHeight), "Aa", CaseSensitivityButtonClick);

            termIncludeButton = area.Button(new Vector2(50, panelHeight), "Or", TermIncludeButtonClick);

            showNamesButton = area.Button(new Vector2(50, panelHeight), "Name", ShowNamesButtonClick);
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
            SearchAllTerms = !SearchAllTerms;
            UpdateToggles();
            QueueUpdateItemList();
        }

        private void CaseSensitivityButtonClick()
        {
            IgnoreCase = !IgnoreCase;
            UpdateToggles();
            QueueUpdateItemList();
        }

        private void UpdateToggles()
        {
            if (SearchAllTerms)
            {
                termIncludeButton.text = "AND";
            }
            else
            {
                termIncludeButton.text = "OR";
            }

            if (IgnoreCase)
            {
                caseSensitivityButton.defaultColor = caseSensitivityButton.hoverColor = Color.white;
            }
            else
            {
                caseSensitivityButton.defaultColor = caseSensitivityButton.hoverColor = Color.gray;
            }

            if (showNames)
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
            search = terms;
            if (terms == "")
            {
                ResetButtonClick();
                return;
            }
            AddToHistory(terms);
            QueueUpdateItemList();
        }

        private void AddToHistory(string terms)
        {
            var index = History.IndexOf(terms);
            if (index >= 0)
            {
                History.RemoveAt(index);
            }

            History.Insert(0, terms);

            var maxHistory = config.MaxHistory.Value;
            if (History.Count > maxHistory)
            {
                History.RemoveRange(maxHistory, History.Count - maxHistory);
            }

            historyDropdown.Choices = History;
        }

        private void QueueUpdateItemList()
        {
            updateItemListQueued = true;
        }

        private void UpdateItemList()
        {
            var termList = search
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
            search = "";
            searchTextField.Value = "";
            controller.ShowAll();
            controller.ResetView();
        }
    }
}