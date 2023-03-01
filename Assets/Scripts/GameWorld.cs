using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

public class GameWorld : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint;

    private const string PlayerPrefabPath = "Player";
    
    public Player CurrentPlayer { get; private set; }


    public void Init()
    {
        if (CurrentPlayer != null) Destroy(CurrentPlayer);
        SpawnPlayer();
    }
    
    private void SpawnPlayer()
    {
        var playerPrefab = Resources.Load<Player>(PlayerPrefabPath);
        CurrentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity, transform);
    }
}