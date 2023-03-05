using System;
using System.ComponentModel;
using Sirenix.OdinInspector;
using UnityEngine;
using World;

namespace Configs
{
    [CreateAssetMenu(menuName = "Data/Factories", fileName = "FactoriesConfig")]
    public class FactoriesConfig : ScriptableObject
    {
        [SerializeField] private FactoryData[] factoryDatas = new FactoryData[0];
        [SerializeField] private float dropToFactoryPeriod = 1;
        [SerializeField] private float randomOffsetRange = 0.4f;
        [SerializeField] private float dropWaitAfterCreateTime = 0.2f;
        
        public FactoryData[] FactoryDatas => factoryDatas;
        public float DropToFactoryPeriod => dropToFactoryPeriod;
        public float RandomOffsetRange => randomOffsetRange;
        public float DropWaitAfterCreateTime => dropWaitAfterCreateTime;
        
    }

    [Serializable]
    public class FactoryData
    {
        [SerializeField] private FactoryType _factoryType;
        [SerializeField] private ItemType _fromItemType;
        [SerializeField] private ItemType _toItemType;

        public ItemType FromItemType => _fromItemType;
        public ItemType ToItemType => _toItemType;
    }
}