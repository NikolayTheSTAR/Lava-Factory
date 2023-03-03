using System.Collections.Generic;
using Configs;
using UnityEngine;
using World;

namespace Mining
{
    public class MiningController : MonoBehaviour
    {
        private const string SourcesLoadPath = "Configs/SourcesConfig";
        private SourcesConfig _sourceConfig;

        public SourcesConfig SourcesConfig
        {
            get
            {
                if (_sourceConfig == null) _sourceConfig = Resources.Load<SourcesConfig>(SourcesLoadPath);

                return _sourceConfig;
            }
        }
        
        private Dictionary<ItemType, ResourceItem> _loadedItemPrefabs;
        
        private string ItemLoadPath(ItemType itemType) => $"Items/{itemType.ToString()}";

        public void Init()
        {
            _loadedItemPrefabs = new Dictionary<ItemType, ResourceItem>();
        }
        
        public ResourceItem GetResourceItemPrefab(ItemType itemType)
        {
            if (_loadedItemPrefabs.ContainsKey(itemType)) return _loadedItemPrefabs[itemType];
            
            var loadedItem = Resources.Load<ResourceItem>(ItemLoadPath(itemType));
            _loadedItemPrefabs.Add(itemType, loadedItem);
            return loadedItem;
        }

        public void OnStartMining(SourceType sourceType, out SourceMiningData miningData)
        {
            miningData = SourcesConfig.SourceDatas[(int)sourceType].MiningData;
        }
    }
}