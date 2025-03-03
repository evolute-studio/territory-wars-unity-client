using System;
using System.Collections;
using System.Collections.Generic;
using Dojo;
using TerritoryWars.Tools;
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
        //public ConnectionType ConnectionType;
        //public GameMode GameMode;
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
        
        private float startConenctionTime;
        

        public void Start()
        {
            CustomSceneManager.Instance.LoadingScreen.SetActive(true);
            StartOnChainMode();
            // CustomSceneManager.Instance.OnLoadScene += SceneLoaded;
            //
            // switch (ConnectionType)
            // {
            //     case ConnectionType.Local:
            //         WorldManager.dojoConfig = LocalConnection;
            //         DojoGameManager.dojoConfig = LocalConnection;
            //         break;
            //     case ConnectionType.RemoteDev:
            //         WorldManager.dojoConfig = RemoteDevConnection;
            //         DojoGameManager.dojoConfig = RemoteDevConnection;
            //         break;
            //     case ConnectionType.RemoteProd:
            //         WorldManager.dojoConfig = RemoteProdConnection;
            //         DojoGameManager.dojoConfig = RemoteProdConnection;
            //         break;
            //     case ConnectionType.None:
            //         break;
            // }
            //
            // //GameContract.contractAddress = WorldManager.dojoConfig.GameContractAddress;
            // //PlayerProfileContract.contractAddress = WorldManager.dojoConfig.PlayerProfileContractAddress;
            //
            // switch (GameMode)
            // {
            //     case GameMode.Offline:
            //         StartOfflineMode();
            //         break;
            //     case GameMode.OnChain:
            //         StartOnChainMode();
            //         break;
            // }
        }

        public void StartOfflineMode()
        {
            
        }
        
        public void StartOnChainMode()
        {
            CustomLogger.LogInfo("Starting OnChain mode");
            
            if (DojoGameManager == null)
            {
                CustomLogger.LogError("DojoGameManager is null!");
                return;
            }
            
            DojoGameManager.Initialize();
            DojoGameGUIController.enabled = UseDojoGUIController;
            
            CustomLogger.LogInfo("Starting TryLoadMenu coroutine");
            StartCoroutine(TryLoadMenu());
            
            #if UNITY_EDITOR && !UNITY_WEBGL
            // if (DojoGameManager.Instance.WorldManager.transform.childCount == 0 ||
            //     DojoGameManager.Instance.LocalBurnerAccount == null)
            // {
            //     CustomLogger.LogInfo("Editor mode. Waiting for LocalBurnerAccount and WorldManager");
            //     DojoGameManager.Instance.WorldManager.synchronizationMaster.OnSynchronized.AddListener(LoadMenu);
            // }
            // else
            // {
            //     LoadMenu();
            // }
            #endif
            #if UNITY_WEBGL && !UNITY_EDITOR

            // CustomLogger.LogInfo("WebGL mode");
            // if (DojoGameManager.Instance.LocalBurnerAccount == null)
            // {
            //     CustomLogger.LogInfo("LocalBurnerAccount is null");
            //     DojoGameManager.Instance.OnLocalPlayerSet.AddListener(LoadMenu);
            // }
            // else
            // {
            //     LoadMenu();
            // }
            #endif
        }

        public IEnumerator TryLoadMenu()
        {
            CustomLogger.LogInfo("Starting TryLoadMenu coroutine");
            int attempts = 0;
            startConenctionTime = Time.time;
            
            while (attempts < 30) 
            {
                attempts++;
                CustomLogger.LogInfo($"Attempt {attempts}: Checking conditions...");
                CustomLogger.LogInfo($"WorldManager synced: {DojoGameManager.Instance.Synced}");
                CustomLogger.LogInfo($"LocalBurnerAccount status: {(DojoGameManager.Instance.LocalBurnerAccount != null ? "Present" : "Null")}");
                
                if (DojoGameManager.Instance.WorldManager == null)
                {
                    CustomLogger.LogError("WorldManager is null!");
                    yield return new WaitForSeconds(1);
                    continue;
                }

                if (DojoGameManager.Instance.LocalBurnerAccount != null)
                {
                    CustomLogger.LogInfo("Conditions met - loading menu");
                    //LoadMenu();
                    break;
                    if(DojoGameManager.Instance.Synced || Time.time - startConenctionTime > 10 || DojoGameManager.Instance.WorldManager.transform.childCount > 0)
                    {
                        
                    }
                }

                yield return new WaitForSeconds(1);
            }
            
            CustomLogger.LogWarning("TryLoadMenu timed out after 30 attempts");
            
            //DojoGameManager.Instance.OnLocalPlayerSet.AddListener(LoadMenu);
        }

        public void LoadMenu()
        {
            CustomLogger.LogInfo("LoadMenu");
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
            CustomSceneManager.Instance.LoadLobby();
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