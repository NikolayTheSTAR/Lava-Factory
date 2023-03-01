using UnityEngine;

namespace World
{
    public class GameWorld : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private ResourceSource[] sources = new ResourceSource[0];

        private const string PlayerPrefabPath = "Player";
    
        public Player CurrentPlayer { get; private set; }


        public void Init()
        {
            if (CurrentPlayer != null) Destroy(CurrentPlayer);
            SpawnPlayer();

            foreach (var source in sources) source.Init();
        }
    
        private void SpawnPlayer()
        {
            var playerPrefab = Resources.Load<Player>(PlayerPrefabPath);
            CurrentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, transform);
        }
    }
}