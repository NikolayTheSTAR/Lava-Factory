using System;
using UnityEngine;
using World;

namespace Configs
{
    [CreateAssetMenu(menuName = "Data/Items", fileName = "ItemsConfig")]
    public class ItemsConfig : ScriptableObject
    {
        [SerializeField] private ItemData[] items = new ItemData[0];
        public ItemData[] Items => items;
    }

    [Serializable]
    public class ItemData
    {
        [SerializeField] private ItemType itemType;
        [Range(0, 5)]
        [SerializeField] private float physicalImpulse = 2;

        public float PhysicalImpulse => physicalImpulse;
    }
}
