using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace TheSTAR.GUI
{
    public class ItemCounter : MonoBehaviour
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI counterText;

        public void SetValue(int value)
        {
            counterText.text = value.ToString();
        
            // todo: animate
        
            gameObject.SetActive(value > 0);
        }
    }
}