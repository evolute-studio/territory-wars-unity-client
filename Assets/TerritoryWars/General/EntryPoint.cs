using Dojo;
using UnityEngine;

namespace TerritoryWars.General
{
    public enum GameMode
    {
        Offline,
        OnChain
    }
    
    public enum ConnectionType
    {
        Local,
        RemoteDev,
        RemoteProd,
        None
    }

    public class EntryPoint : MonoBehaviour
    {
        public ConnectionType ConnectionType;
        public GameMode GameMode;
        public WorldManager WorldManager;
        public DojoGameManager DojoGameManager;
        public DojoGameController DojoGameGUIController;
        public bool UseDojoGUIController = false;
        
        [Header("Connections")]
        public WorldManagerData LocalConnection;
        public WorldManagerData RemoteDevConnection;
        public WorldManagerData RemoteProdConnection;
        
        [Header("Contracts")]
        public Game GameContract;
        public Player_profile_actions PlayerProfileContract;
        

        public void Start()
        {
            CustomSceneManager.Instance.OnLoadScene += SceneLoaded;

            switch (ConnectionType)
            {
                case ConnectionType.Local:
                    WorldManager.dojoConfig = LocalConnection;
                    DojoGameManager.dojoConfig = LocalConnection;
                    break;
                case ConnectionType.RemoteDev:
                    WorldManager.dojoConfig = RemoteDevConnection;
                    DojoGameManager.dojoConfig = RemoteDevConnection;
                    break;
                case ConnectionType.RemoteProd:
                    WorldManager.dojoConfig = RemoteProdConnection;
                    DojoGameManager.dojoConfig = RemoteProdConnection;
                    break;
                case ConnectionType.None:
                    break;
            }
            
            GameContract.contractAddress = WorldManager.dojoConfig.GameContractAddress;
            PlayerProfileContract.contractAddress = WorldManager.dojoConfig.PlayerProfileContractAddress;
            
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