using Mining;
using TheSTAR.Data;
using TheSTAR.GUI;
using TheSTAR.GUI.Screens;
using TheSTAR.Input;
using UnityEngine;
using World;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameWorld world;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private InputController input;
    [SerializeField] private DropItemsContainer drop;
    [SerializeField] private MiningController mining;
    [SerializeField] private DataController data;
    [SerializeField] private TransactionsController transactions;
    [SerializeField] private GuiController gui;

    /// <summary>
    /// Main logic entry point
    /// </summary>
    private void Start() => Init();

    private void Init()
    {
        mining.Init();
        world.Init(drop, mining, transactions);
        cameraController.FocusTo(world.CurrentPlayer);
        gui.Init(out var trs);

        var gameScreen = gui.FindScreen<GameScreen>();
        gameScreen.Init(mining);
        
        input.Init(gameScreen.JoystickContainer, world.CurrentPlayer);
        transactions.Init(trs, data);
        drop.Init(transactions, mining, world.CurrentPlayer, world.CurrentPlayer.StopTransaction);
    }
}