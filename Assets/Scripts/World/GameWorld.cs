using Mining;
using UnityEngine;

namespace World
{
    public class GameWorld : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private ResourceSource[] sources = new ResourceSource[0];
        [SerializeField] private Factory[] factories = new Factory[0];

        private const string PlayerPrefabPath = "Player";
    
        public Player CurrentPlayer { get; private set; }

        private MiningController _miningController;
        private DropItemsContainer _dropItemsContainer;
        private TransactionsController _transactions;

        public void Init(DropItemsContainer dropItemsContainer, MiningController miningController, TransactionsController transactions)
        {
            _miningController = miningController;
            _dropItemsContainer = dropItemsContainer;
            _transactions = transactions;
            
            if (CurrentPlayer != null) Destroy(CurrentPlayer);
            SpawnPlayer();

            foreach (var source in sources)
            {
                var sourceType = source.SourceType;
                var miningData = _miningController.SourcesConfig.SourceDatas[(int)sourceType].MiningData;
                source.Init(miningData, dropItemsContainer.DropFromSource, (s) =>
                {
                    CurrentPlayer.StopMining();
                    _miningController.StartSourceRecovery(s);
                }, CurrentPlayer.RetryInteract);
            }

            foreach (var factory in factories)
            {
                //factory.Init(dropItemsContainer.DropToFactory);
            }
        }
    
        private void SpawnPlayer()
        {
            var playerPrefab = Resources.Load<Player>(PlayerPrefabPath);
            CurrentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, transform);
            CurrentPlayer.Init(_miningController.OnStartMining, _dropItemsContainer.DropToFactory, _transactions.FactoriesConfig.DropToFactoryPeriod);
        }
    }
}