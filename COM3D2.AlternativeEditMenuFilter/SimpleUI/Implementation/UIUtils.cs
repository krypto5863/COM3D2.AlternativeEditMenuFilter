using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.SimpleUI.Implementation
{
    public static class UIUtils
    {
        private static readonly Dictionary<string, string> resourcePaths =
        new Dictionary<string, string>() {
            {"AtlasCommon", "CommonUI/Atlas/AtlasCommon" },
            {"NotoSansCJKjp-DemiLight", "font/notosanscjkjp-hinted/notosanscjkjp-demilight" },
        };

        private static readonly Dictionary<string, UIAtlas> atlasCache = new Dictionary<string, UIAtlas>();

        public static UIAtlas GetAtlas(string name)
        {
            if (!atlasCache.ContainsKey(name))
            {
                atlasCache[name] = null;

                if (resourcePaths.ContainsKey(name))
                {
                    var path = resourcePaths[name];
                    var prefab = Resources.Load<UIAtlas>(path);
                    if (prefab != null)
                    {
                        atlasCache[name] = UnityEngine.Object.Instantiate(prefab);
                        UnityEngine.Object.DontDestroyOnLoad(atlasCache[name]);
                    }
                }
            }

            return atlasCache[name];
        }

        public static Font GetFont(string name)
        {
            if (resourcePaths.ContainsKey(name))
            {
                var path = resourcePaths[name];
                return Resources.Load<Font>(path);
            }

            return null;
        }
    }
}