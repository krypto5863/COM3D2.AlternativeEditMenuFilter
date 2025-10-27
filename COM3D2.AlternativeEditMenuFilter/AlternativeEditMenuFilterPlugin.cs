using BepInEx;
using BepInEx.Logging;
using COM3D2.AlternativeEditMenuFilter.Translation.XUATProvider;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.AlternativeEditMenuFilter
{
    [BepInPlugin("org.bepinex.plugins.com3d2.alternativeeditmenufilter", "AlternativeEditMenuFilter", "1.0")]
    public class AlternateEditMenuFilterPlugin : BaseUnityPlugin
    {
        public static AlternateEditMenuFilterPlugin Instance { get; private set; }

        public ITranslationProvider TranslationProvider
        {
            get; protected set;
        }

        public new AlternativeEditMenuFilterConfig Config { get; private set; }

        private void Awake()
        {
            if (Instance != null) throw new Exception("Already instantiated");
            Instance = this;
            DontDestroyOnLoad(this);

            Config = new AlternativeEditMenuFilterConfig(base.Config);

            TranslationProvider = XUATTranslationProvider.Create();
            if (TranslationProvider == null)
            {
                Log.LogWarning("XUAT is not available, machine translated text search not available");
                TranslationProvider = new DummyTranslationProvider();
            }

            SceneManager.sceneLoaded += OnChangedSceneLevel;
        }

        private void OnChangedSceneLevel(Scene scenename, LoadSceneMode SceneMode)
        {
            if (scenename.name == "SceneEdit")
            {
                InstallMenu();
            }
        }

        private void InstallMenu()
        {
            //StopAllCoroutines();
            var itemMenu = InstallMenu<EditMenuPanelFilter>("ScrollPanel-MenuItem");
            itemMenu.Init(Config.ItemSearchConfig, new Vector3(-575, 520));

            var setMenu = InstallMenu<EditMenuPanelFilter>("ScrollPanel-SetItem");
            setMenu.Init(Config.ItemSetSearchConfig, new Vector3(-575, 520));

            var presetMenu = InstallMenu<PresetPanelFilter>("PresetPanel/PresetViewer");
            presetMenu.Init(Config.PresetSearchConfig, new Vector3(-575, 520));
        }

        private T InstallMenu<T>(string panelName)
            where T : MonoBehaviour
        {
            Transform transform = GameObject.Find("UI Root").transform;
            if (transform == null)
            {
                Log.LogVerbose("Cannot find UI Root for {0}", panelName);
                return null;
            }

            Transform transform2 = transform.Find(panelName);
            if (transform2)
            {
                Log.LogVerbose("Found panel {0}", panelName);
                GameObject gameObject = NGUITools.AddChild(transform2.gameObject);
                gameObject.name = typeof(T).Name;
                var control = gameObject.AddComponent<T>();
                return control;
            }
            return null;
        }

        internal new ManualLogSource Logger => base.Logger;
    }
}