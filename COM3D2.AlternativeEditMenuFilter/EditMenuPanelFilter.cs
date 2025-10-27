using COM3D2.SimpleUI;
using COM3D2.SimpleUI.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace COM3D2.AlternativeEditMenuFilter
{
    internal class PendingTranslation
    {
        public EditMenuPanelItem Item { get; set; }
        public ITranslationAsyncResult Result { get; set; }
    }

    public class EditMenuPanelFilter : MonoBehaviour
    {
        private EditMenuPanelController controller;

        private string search = "";
        private bool updateItemListQueued = false;

        private IButton caseSensitivityButton;
        private IButton termIncludeButton;
        private ITextField searchTextField;
        private IDropdown historyDropdown;
        private IGenericDropdown itemTypeFilterDropdown;

        private readonly List<string> History = new List<string>();

        private MenuSearchConfig config;

        private bool SearchInName => SearchTextMode == SearchTextModeEnum.NAME || SearchTextMode == SearchTextModeEnum.ALL;
        private bool SearchInInfo => SearchTextMode == SearchTextModeEnum.DESCRIPTION || SearchTextMode == SearchTextModeEnum.ALL;

        private bool SearchInFilename => SearchTextMode == SearchTextModeEnum.FILENAME || SearchTextMode == SearchTextModeEnum.ALL;
        private bool SearchInDirectoryName => SearchTextMode == SearchTextModeEnum.DIRECTORYNAME || SearchTextMode == SearchTextModeEnum.ALL;

        private bool SearchLocalized
        {
            get => config.SearchLocalized.Value;
            set => config.SearchLocalized.Value = value;
        }

        private bool SearchMTL => SearchLocalized && config.SearchMTL.Value;

        private bool SendMTL => SearchMTL && config.SendMTL.Value;

        private bool SearchAllTerms
        {
            get => config.SearchAllTerms.Value;
            set => config.SearchAllTerms.Value = value;
        }

        private bool IncludeMods => ItemTypeFilter == ItemTypeFilterEnum.MOD || ItemTypeFilter == ItemTypeFilterEnum.ALL;

        private bool IncludeVanilla => ItemTypeFilter == ItemTypeFilterEnum.VANILLA || ItemTypeFilter == ItemTypeFilterEnum.ALL;

        private bool IncludeCompat => ItemTypeFilter == ItemTypeFilterEnum.COMPAT || ItemTypeFilter == ItemTypeFilterEnum.ALL;

        private bool IgnoreCase
        {
            get => config.IgnoreCase.Value;
            set => config.IgnoreCase.Value = value;
        }

        private ItemTypeFilterEnum ItemTypeFilter
        {
            get => config.ItemTypeFilter.Value;
            set => config.ItemTypeFilter.Value = value;
        }

        private SearchTextModeEnum SearchTextMode
        {
            get => config.SearchTextMode.Value;
            set => config.SearchTextMode.Value = value;
        }

        private ITranslationProvider TranslationProvider => AlternateEditMenuFilterPlugin.Instance.TranslationProvider;

        private readonly List<PendingTranslation> pendingTranslations = new List<PendingTranslation>();

        public void Init(MenuSearchConfig config, Vector3 localPosition)
        {
            controller = new EditMenuPanelController(gameObject.transform.parent.gameObject);
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

        private void OnEnable()
        {
            QueueUpdateItemList();
        }

        private void OnDisable()
        {
            Log.LogVerbose("Saving search history");
            config.History.Value = String.Join("\n", History.ToArray());
        }

        private void Update()
        {
            if (updateItemListQueued)
            {
                updateItemListQueued = false;
                TranslationProvider.ResetAsyncQueue();
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

            area.GenericDropdown(new Vector2(50, panelHeight), "ALL")
                .Choice(SearchTextModeEnum.ALL, "[All] text", "All")
                .Choice(SearchTextModeEnum.NAME, "[Name]", "Name")
                .Choice(SearchTextModeEnum.DESCRIPTION, "[Info]", "Info")
                .Choice(SearchTextModeEnum.FILENAME, "[Fn] Filename", "Fn")
                //                .Choice(SearchTextModeEnum.DIRECTORYNAME, "[Dir] Full directory path", "Dir")
                .SetValue(SearchTextMode)
                .SetUpdateTextOnValuechange(true)
                .AddChangeCallback(o =>
                {
                    if (o is SearchTextModeEnum mode)
                    {
                        SearchTextMode = mode;
                        QueueUpdateItemList();
                        Log.LogVerbose("New mode is {0}", mode);
                    }
                });

            itemTypeFilterDropdown = area.GenericDropdown(new Vector2(50, panelHeight), "All")
                .Choice(ItemTypeFilterEnum.ALL, "[All] items", "All")
                .Choice(ItemTypeFilterEnum.VANILLA, "[COM] Vanilla COM3D2", "COM")
                .Choice(ItemTypeFilterEnum.COMPAT, "[CM] Compat/CM", "CM")
                .Choice(ItemTypeFilterEnum.MOD, "[Mod]s", "Mod")
                .SetValue(ItemTypeFilter)
                .SetUpdateTextOnValuechange(true);

            itemTypeFilterDropdown.AddChangeCallback(o =>
                {
                    if (o is ItemTypeFilterEnum t)
                    {
                        ItemTypeFilter = t;
                        QueueUpdateItemList();
                        Log.LogVerbose("New filter type is {0}", t);
                    }
                });

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
            controller.HidePanel();
        }

        private void UpdateItemList()
        {
            var termList = search
                .Split(' ')
                .Where(t => !string.IsNullOrEmpty(t))
                .Select(t => t.Trim())
                .ToArray();

            Log.LogVerbose("Clearing pending translations");
            pendingTranslations.Clear();
            TranslationProvider.ResetAsyncQueue();

            Log.LogVerbose("Performing filter");
            foreach (var item in controller.GetAllItems())
            {
                var matchesTermList = termList.Length == 0 || FilterItem(item, termList);
                item.Visible = matchesTermList && FilterType(item);
            }

            controller.ShowPanel();
            controller.ResetView();
        }

        private bool FilterItem(EditMenuPanelItem item, string[] termList)
        {
            var inName = SearchInName;
            var inInfo = SearchInInfo;
            var inFilename = SearchInFilename;
            var inDirectoryname = SearchInDirectoryName;
            var translatedNameAvailable = false;
            var translatedInfoAvailable = false;

            if (SearchInName)
            {
                var inOriginalName = StringContains(item.Name, termList);
                var inLocalizedName = SearchLocalized && StringContains(item.LocalizedName, termList);
                var inTranslatedName = SearchMTL && TranslationContains(item.Name, termList, out translatedNameAvailable);
                inName = inOriginalName || inLocalizedName || inTranslatedName;
            }

            if (SearchInInfo)
            {
                var inOriginalInfo = StringContains(item.Info, termList);
                var inLocalizedInfo = SearchLocalized && StringContains(item.LocalizedInfo, termList);
                var inTranslatedInfo = SearchMTL && TranslationContains(item.Info, termList, out translatedInfoAvailable);
                inInfo = inOriginalInfo || inLocalizedInfo || inTranslatedInfo;
            }

            if (SearchInFilename)
            {
                inFilename = StringContains(item.Filename, termList);
            }

            if (SearchInDirectoryName)
            {
                var directoryName = GetDirectoryName(item.Filename);
                inDirectoryname = StringContains(directoryName, termList);
            }

            return inName || inInfo || inFilename || inDirectoryname;
        }

        private void QueueTranslation(EditMenuPanelItem item, string text)
        {
            if (SendMTL)
            {
                Log.LogVerbose("Sending string for translation: {0}", text);
                pendingTranslations.Add(new PendingTranslation()
                {
                    Item = item,
                    Result = TranslationProvider.TranslateAsync(text)
                });
            }
        }

        private bool FilterType(EditMenuPanelItem item)
        {
            if (!IncludeMods && item.IsMod)
            {
                Log.LogVerbose("Hiding {0}, mods excluded", item.Filename);
                return false;
            }

            if (!IncludeCompat && item.IsCompat)
            {
                Log.LogVerbose("Hiding {0}, compat excluded", item.Filename);
                return false;
            }

            if (!IncludeVanilla && item.IsVanilla && !item.IsCompat)
            {
                Log.LogVerbose("Hiding {0}, vanilla excluded", item.Filename);
                return false;
            }

            return true;
        }

        private string GetDirectoryName(string filename)
        {
            // TODO: Implement this
            return "";
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

        private bool TranslationContains(string str, string[] terms, out bool available)
        {
            var result = TranslationProvider.Translate(str);
            if (result.IsTranslationSuccessful)
            {
                available = true;
                return StringContains(result.TranslatedText, terms);
            }
            available = false;
            return false;
        }

        private void ResetButtonClick()
        {
            search = "";
            searchTextField.Value = "";
            ItemTypeFilter = ItemTypeFilterEnum.ALL;
            itemTypeFilterDropdown.SetValue(ItemTypeFilter);
            TranslationProvider.ResetAsyncQueue();
            controller.ShowAll();
            controller.ResetView();
        }

        public enum SearchTextModeEnum
        {
            ALL,
            NAME,
            DESCRIPTION,
            FILENAME,
            DIRECTORYNAME,
        }

        public enum ItemTypeFilterEnum
        {
            ALL,
            VANILLA,
            COMPAT,
            MOD
        }
    }
}