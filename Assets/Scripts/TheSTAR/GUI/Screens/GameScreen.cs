using TheSTAR.Input;
using UnityEngine;
using World;

namespace TheSTAR.GUI.Screens
{
    public class GameScreen : GuiScreen, ITransactionReactable
    {
        [SerializeField] private JoystickContainer joystickContainer;
        [SerializeField] private ItemCounter counter;

        public JoystickContainer JoystickContainer => joystickContainer;
        
        public void OnTransactionReact(ItemType itemType, int finalValue)
        {
            counter.SetValue(finalValue);   
        }
    }
}