using System.Collections.Generic;
using Configs;
using TheSTAR.Data;
using UnityEngine;
using World;
using TheSTAR.Utility;

public class TransactionsController : MonoBehaviour
{
    private List<ITransactionReactable> _transactionReactables;
    private DataController _data;

    private const string FactoriesConfigPath = "Configs/FactoriesConfig";
    private FactoriesConfig _factoriesConfig;
    public FactoriesConfig FactoriesConfig
    {
        get
        {
            if (_factoriesConfig == null) _factoriesConfig = Resources.Load<FactoriesConfig>(FactoriesConfigPath);
            return _factoriesConfig;
        }
    }
    
    public void Init(List<ITransactionReactable> trs, DataController data)
    {
        _transactionReactables = trs;
        _data = data;
        
        InitReaction();
    }

    public void AddItem(ItemType itemType, int count = 1, bool autoSave = true)
    {
        _data.gameData.AddItems(itemType, count, out int result);
        if (autoSave) _data.Save();
        
        Reaction(itemType, result);
    }
    
    public void ReduceItem(ItemType itemType, int count = 1, bool autoSave = false)
    {
        _data.gameData.AddItems(itemType, -count, out int result);
        if (autoSave) _data.Save();
        
        Reaction(itemType, result);
    }

    private void InitReaction()
    {
        var itemTypes = EnumUtility.GetValues<ItemType>();
        
        foreach (var tr in _transactionReactables)
            foreach (var itemType in itemTypes)
                tr.OnTransactionReact(itemType, _data.gameData.GetItemCount(itemType));
    }

    private void Reaction(ItemType itemType, int finalValue)
    {
        foreach (var tr in _transactionReactables) tr.OnTransactionReact(itemType, finalValue);
    }
}

public interface ITransactionReactable
{
    void OnTransactionReact(ItemType itemType, int finalValue);
}