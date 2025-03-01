using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class CancelGamePopup : MonoBehaviour
    {
        public GameObject CancelGamePopupGO;
        public Button CancelButton;
        public Button ConfirmButton;


        
        
        public void Initialization()
        {
            
        }
        
        public void SetCancelPopupGameActive(bool active)
        {
            CancelGamePopupGO.SetActive(active);
        }
    }
}