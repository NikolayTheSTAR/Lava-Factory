using Mining;
using TheSTAR.Input;
using UnityEngine;
using World;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameWorld world;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private InputController inputController;
    [SerializeField] private DropItemsContainer dropItemsContainer;
    [SerializeField] private MiningController miningController;

    /// <summary>
    /// Main logic entry point
    /// </summary>
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        miningController.Init();
        world.Init(dropItemsContainer, miningController);
        cameraController.FocusTo(world.CurrentPlayer);
        inputController.Init(world.CurrentPlayer);
        dropItemsContainer.Init(miningController, world.CurrentPlayer);
    }
}