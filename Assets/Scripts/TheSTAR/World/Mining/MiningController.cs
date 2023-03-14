using System;
using System.Collections.Generic;
using Configs;
using UnityEngine;
using World;
using TheSTAR.Utility;

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

        private const string ItemsConfigLoadPath = "Configs/ItemsConfig";
        private ItemsConfig _itemsConfig;
        public ItemsConfig ItemsConfig
        {
            get
            {
                if (_itemsConfig == null) _itemsConfig = Resources.Load<ItemsConfig>(ItemsConfigLoadPath);
                return _itemsConfig;
            }
        }
        
        private Dictionary<ItemType, ResourceItem> _loadedItemPrefabs;
        private List<RecoveryData> _recoveryDatas = new List<RecoveryData>();
        private bool _isWaitForRecovery = false;

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

        public void StartSourceRecovery(ResourceSource source)
        {
            var recoveryTimeSpan = new TimeSpan(
                    source.SourceData.MiningData.RecoveryTime.Hours,
                    source.SourceData.MiningData.RecoveryTime.Minutes,
                    source.SourceData.MiningData.RecoveryTime.Seconds);

            var recoveryDateTime = DateTime.Now + recoveryTimeSpan;

            var recoveryData = new RecoveryData(source, recoveryDateTime);

            _recoveryDatas.Add(recoveryData);

            if (!_isWaitForRecovery)
            {
                TimeUtility.Wait((float)recoveryTimeSpan.TotalSeconds, CheckRecovery);
                _isWaitForRecovery = true;
            }
        }

        private void CheckRecovery()
        {
            bool breakCheck = false;

            while (!breakCheck)
            {
                if (_recoveryDatas.Count == 0)
                {
                    breakCheck = true;
                    continue;
                }

                var testRecoveryData = _recoveryDatas[0];

                if (DateTime.Now < testRecoveryData.recoveryTime)
                {
                    breakCheck = true;
                    continue;
                }

                testRecoveryData.source.Recovery();
                _recoveryDatas.Remove(testRecoveryData);
            }

            if (_recoveryDatas.Count > 0)
            {
                var source = _recoveryDatas[0];
                var waitTime = source.recoveryTime - DateTime.Now;

                TimeUtility.Wait((float)waitTime.TotalSeconds, CheckRecovery);
            }
            else _isWaitForRecovery = false;
        }

        [Serializable]
        private class RecoveryData
        {
            public ResourceSource source { get; private set; }
            public DateTime recoveryTime { get; private set; }

            public RecoveryData(ResourceSource source, DateTime time)
            {
                this.source = source;
                this.recoveryTime = time;
            }
        }
    }
}