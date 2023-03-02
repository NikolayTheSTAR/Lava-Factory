using UnityEngine;

namespace World
{
    public class GameWorld : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private ResourceSource[] sources = new ResourceSource[0];
        [SerializeField] private DropItemsContainer dropItemsContainer;

        private const string PlayerPrefabPath = "Player";
    
        public Player CurrentPlayer { get; private set; }


        public void Init()
        {
            if (CurrentPlayer != null) Destroy(CurrentPlayer);
            SpawnPlayer();
            
            dropItemsContainer.Init(CurrentPlayer);
            
            foreach (var source in sources) source.Init(dropItemsContainer.DropItemFromSource);
        }
    
        private void SpawnPlayer()
        {
            var playerPrefab = Resources.Load<Player>(PlayerPrefabPath);
            CurrentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, transform);
            CurrentPlayer.Init();
        }
    }
}