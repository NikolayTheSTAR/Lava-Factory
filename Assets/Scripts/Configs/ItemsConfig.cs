using System;
using UnityEngine;
using World;

namespace Configs
{
    [CreateAssetMenu(menuName = "Data/Items", fileName = "ItemsConfig")]
    public class ItemsConfig : ScriptableObject
    {
        [SerializeField] private ItemData[] items = new ItemData[0];
    }

    [Serializable]
    public class ItemData
    {
        [SerializeField] private ItemType _itemType;
    }
}
