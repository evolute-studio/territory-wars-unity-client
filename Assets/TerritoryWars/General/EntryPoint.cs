using UnityEngine;

namespace TerritoryWars.General
{
    public enum GameMode
    {
        Offline,
        OnChain
    }

    public class EntryPoint : MonoBehaviour
    {
        public GameMode GameMode;
        public DojoGameManager DojoGameManager;
        public DojoGameController DojoGameGUIController;
        public bool UseDojoGUIController = false;

        public void Start()
        {
            switch (GameMode)
            {
                case GameMode.Offline:
                    StartOfflineMode();
                    break;
                case GameMode.OnChain:
                    StartOnChainMode();
                    break;
            }
        }

        public void StartOfflineMode()
        {
            
        }
        
        public void StartOnChainMode()
        {
            DojoGameManager.Initialize();
            DojoGameGUIController.enabled = UseDojoGUIController;
            
        }
    }


}