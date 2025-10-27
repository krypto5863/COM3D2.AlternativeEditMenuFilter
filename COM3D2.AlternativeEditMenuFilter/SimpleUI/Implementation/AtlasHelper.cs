using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.SimpleUI.Implementation
{
    public class AtlasHelper : MonoBehaviour
    {
        private readonly Dictionary<string, UIAtlas> lookup = new Dictionary<string, UIAtlas>();

        private bool loadComplete = false;
        private string[] pendingAtlasLoadList;

        public void Init(string[] atlasNameList)
        {
            this.pendingAtlasLoadList = atlasNameList;
        }

        public void Start()
        {
            var result = Resources.LoadAsync<UIAtlas>("");
            //result.asset
        }

        public UIAtlas GetAtlas(string name)
        {
            if (!loadComplete)
            {
            }
            return null;
        }
    }
}