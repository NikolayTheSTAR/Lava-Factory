using System;
using TheSTAR.Input;
using UnityEngine;
using World;

namespace TheSTAR.GUI.Screens
{
    public class GameScreen : GuiScreen, ITransactionReactable
    {
        [SerializeField] private JoystickContainer joystickContainer;
        [SerializeField] private ItemCounter[] counters = new ItemCounter[0];

        public JoystickContainer JoystickContainer => joystickContainer;
        
        public void OnTransactionReact(ItemType itemType, int finalValue)
        {
            var counter = Array.Find(counters, info => info.ItemType == itemType);
            if (counter == null) return;
            
            counter.SetValue(finalValue);   
        }
    }
}