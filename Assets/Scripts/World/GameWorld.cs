using Mining;
using UnityEngine;

namespace World
{
    public class GameWorld : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private ResourceSource[] sources = new ResourceSource[0];

        private const string PlayerPrefabPath = "Player";
    
        public Player CurrentPlayer { get; private set; }

        private MiningController _miningController;

        public void Init(DropItemsContainer dropItemsContainer, MiningController miningController)
        {
            _miningController = miningController;
            
            if (CurrentPlayer != null) Destroy(CurrentPlayer);
            SpawnPlayer();
            
            foreach (var source in sources) source.Init(dropItemsContainer.DropFromSource);
        }
    
        private void SpawnPlayer()
        {
            var playerPrefab = Resources.Load<Player>(PlayerPrefabPath);
            CurrentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, transform);
            CurrentPlayer.Init(_miningController.OnStartMining);
        }
    }
}