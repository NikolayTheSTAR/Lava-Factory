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

        private string ItemLoadPath(ItemType itemType) => $"Items/{itemType}";

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

            int urgency = GetSourceUrgency(recoveryData);
            if (urgency == -1) _recoveryDatas.Add(recoveryData);
            else
            {
                _recoveryDatas.Insert(urgency, recoveryData);

                if (urgency == 0)
                {
                    TimeUtility.Wait((float)recoveryTimeSpan.TotalSeconds, CheckRecovery);
                    recoveryData.OnStartWaiting();
                }
            }
        }

        private int GetSourceUrgency(RecoveryData current)
        {
            if (_recoveryDatas.Count == 0) return 0;

            for (int i = 0; i < _recoveryDatas.Count; i++)
            {
                if (current.RecoveryTime < _recoveryDatas[i].RecoveryTime) return i;
            }

            return -1;
        }

        private void CheckRecovery()
        {
            bool breakCheck = false;

            // recovery now
            while (!breakCheck)
            {
                if (_recoveryDatas.Count == 0)
                {
                    breakCheck = true;
                    continue;
                }

                var testRecoveryData = _recoveryDatas[0];

                if (DateTime.Now < testRecoveryData.RecoveryTime)
                {
                    breakCheck = true;
                    continue;
                }

                testRecoveryData.Source.Recovery();
                _recoveryDatas.Remove(testRecoveryData);
            }

            // next recovery for waiting
            if (_recoveryDatas.Count > 0)
            {
                var recoveryData = _recoveryDatas[0];
                if (recoveryData.IsWaiting) return;

                var waitTime = recoveryData.RecoveryTime - DateTime.Now;

                TimeUtility.Wait((float)waitTime.TotalSeconds, CheckRecovery);
                recoveryData.OnStartWaiting();
            }
        }

        [Serializable]
        private class RecoveryData
        {
            public ResourceSource Source { get; private set; }
            public DateTime RecoveryTime { get; private set; }
            public bool IsWaiting { get; private set; }

            public RecoveryData(ResourceSource source, DateTime time)
            {
                Source = source;
                RecoveryTime = time;
            }

            public void OnStartWaiting() => IsWaiting = true;
        }
    }
}