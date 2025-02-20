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
            CustomSceneManager.Instance.OnLoadScene += SceneLoaded;
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
        
        private void SceneLoaded(string name)
        {
            if (name == CustomSceneManager.Instance.Menu)
            {
                
            }
            else if (name == CustomSceneManager.Instance.Session)
            {
                //SessionManager.Instance.Initialize();

            }
        }
        
        private void OnDestroy()
        {
            CustomSceneManager.Instance.OnLoadScene -= SceneLoaded;
        }
    }


}