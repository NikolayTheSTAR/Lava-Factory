using TheSTAR.Input;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameWorld world;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private InputController inputController;

    /// <summary>
    /// Main logic entry point
    /// </summary>
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        world.Init();
        cameraController.FocusTo(world.CurrentPlayer);
        inputController.Init(world.CurrentPlayer.SetDestination);
    }
}