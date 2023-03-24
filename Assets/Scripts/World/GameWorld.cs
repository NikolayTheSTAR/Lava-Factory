using System.Collections.Generic;
using System.Linq;
using Configs;
using Mining;
using UnityEngine;
using TheSTAR.Data;

namespace World
{
    public class GameWorld : MonoBehaviour, IPreparable
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private ResourceSource[] sources = new ResourceSource[0];
        [SerializeField] private Factory[] factories = new Factory[0];
        [SerializeField] private Player playerPrefab;
        [SerializeField] private int levelIndex;
    
        public Player CurrentPlayer { get; private set; }

        private MiningController _miningController;
        private DropItemsContainer _dropItemsContainer;
        private TransactionsController _transactions;

        public void Init(DropItemsContainer dropItemsContainer, MiningController miningController, TransactionsController transactions, DataController data)
        {
            _miningController = miningController;
            _dropItemsContainer = dropItemsContainer;
            _transactions = transactions;
            
            if (CurrentPlayer != null) Destroy(CurrentPlayer.gameObject);
            SpawnPlayer();

            SourceType sourceType;
            SourceData sourceData;
            foreach (var source in sources)
            {
                sourceType = source.SourceType;
                sourceData = _miningController.SourcesConfig.SourceDatas[(int)sourceType];
                source.Init(sourceData, dropItemsContainer.DropFromSenderToPlayer, (s) =>
                {
                    CurrentPlayer.StopInteract();
                    _miningController.StartSourceRecovery(s);
                },
                CurrentPlayer.RetryInteract);
            }

            FactoryData factoryData = null;
            Factory factory;

            for (int i = 0; i < factories.Length; i++)
            {
                factory = factories[i];

                if (factory == null) continue;
                
                factoryData = transactions.FactoriesConfig.FactoryDatas[(int)factory.FactoryType];
                factory.Init(i, factoryData, dropItemsContainer.DropFromSenderToPlayer, data.gameData.GetFactoryStorageValue(levelIndex, i));

                factory.OnAddItemToStorageEvent += (index, value) =>
                {
                    data.gameData.AddItemToFactoryStorage(levelIndex, index, value);
                    data.Save();
                };

                factory.OnEmptyStorageEvent += (index) =>
                {
                    data.gameData.EmptyFactoryStorage(levelIndex, index);
                    data.Save();
                };
            }
        }
    
        private void SpawnPlayer()
        {
            CurrentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, transform);
            CurrentPlayer.Init(_transactions, _dropItemsContainer.DropToFactory, _transactions.FactoriesConfig.DropToFactoryPeriod);
        }
        
        [ContextMenu("Prepare")]
        public void Prepare()
        {
#if UNITY_EDITOR
            var allSources = GameObject.FindGameObjectsWithTag("Source");
            sources = allSources.Select(s => s.GetComponent<ResourceSource>()).Where(s => s != null).ToArray();

            var allFactories = GameObject.FindGameObjectsWithTag("Factory");
            factories = allFactories.Select(f => f.GetComponent<Factory>()).Where(f => f != null).ToArray();
#endif
        }
    }
}