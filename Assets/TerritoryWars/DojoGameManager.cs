using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using bottlenoselabs.C2CS.Runtime;
using Dojo;
using Dojo.Starknet;
using UnityEngine;

// Fix to use Records in Unity ref. https://stackoverflow.com/a/73100830
using System.ComponentModel;
using TerritoryWars.General;
using UnityEngine.Serialization;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}

namespace TerritoryWars
{
    public class DojoGameManager : MonoBehaviour
    {
        public static DojoGameManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        public WorldManager WorldManager;

        [SerializeField] WorldManagerData dojoConfig;
        [SerializeField] GameManagerData gameManagerData;
        
        public Game GameSystem;

        public BurnerManager burnerManager;

        public JsonRpcClient provider;
        public Account masterAccount;
        
        public Account LocalBurnerAccount { get; private set; }

        public bool IsLocalPlayer;
        
        public DojoSessionManager SessionManager;


        public void Initialize()
        {
            provider = new JsonRpcClient(dojoConfig.rpcUrl);
            masterAccount = new Account(provider, new SigningKey(gameManagerData.masterPrivateKey),
                new FieldElement(gameManagerData.masterAddress));
            burnerManager = new BurnerManager(provider, masterAccount);

            WorldManager.synchronizationMaster.OnEventMessage.AddListener(OnDojoEventReceived);
            WorldManager.synchronizationMaster.OnSynchronized.AddListener(OnSynchronized);
            WorldManager.synchronizationMaster.OnEntitySpawned.AddListener(SpawnEntity);
            WorldManager.synchronizationMaster.OnModelUpdated.AddListener(ModelUpdated);
            
            CreateAccount();
        }
        
        private async void CreateAccount()
        {
            try
            {
                LocalBurnerAccount = await burnerManager.DeployBurner();
                // if (burnerManager.Burners.Count == 0)
                // {
                //     LocalBurnerAccount = await burnerManager.DeployBurner();
                // }
                // else
                // {
                //     //use last burner account
                //     LocalBurnerAccount = burnerManager.Burners.Last();
                // }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public GameObject[] GetGames() => WorldManager.Entities<evolute_duel_Game>();
        
        public async void CreateGame()
        {
            try
            {
                var txHash = await GameSystem.create_game(LocalBurnerAccount);
                Debug.Log($"Create Game: {txHash}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public async void JoinGame(FieldElement hostPlayer)
        {
            try
            {
                var txHash = await GameSystem.join_game(LocalBurnerAccount, hostPlayer);
                Debug.Log($"Join Game: {txHash}");
                SessionManager = new DojoSessionManager(this);
                CustomSceneManager.Instance.LoadSession();
                
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        private void ModelUpdated(ModelInstance modelInstance)
        {
            Debug.Log($"Model updated: {modelInstance.transform.name}");
            if (modelInstance.transform.TryGetComponent(out evolute_duel_Game gameModel))
            {
                CheckStartSession();
            }
            if(modelInstance.transform.TryGetComponent(out evolute_duel_Board boardModel))
            {
                SessionManager.CheckBoardUpdate();
            }
            
        }

        private void CheckStartSession()
        {
            if (CustomSceneManager.Instance.CurrentScene == CustomSceneManager.Instance.Session) return;
            
            GameObject[] games = WorldManager.Entities<evolute_duel_Game>();
            evolute_duel_Game hostedGame = null;
            if (games.Length == 0) return;

            foreach (var game in games)
            {
                if (game.TryGetComponent(out evolute_duel_Game gameModel))
                {
                    if (gameModel.player.Hex() == LocalBurnerAccount.Address.Hex())
                    {
                        hostedGame = gameModel;
                        break;
                    }
                }
            }
            if (hostedGame == null) return;
            
            var hostPlayer = hostedGame.player;
            Debug.Log("Host player: " + hostPlayer.Hex());
            Debug.Log("Local player: " + LocalBurnerAccount.Address);
            Debug.Log("Host player is local: " + (hostPlayer == LocalBurnerAccount.Address));
            Debug.Log("Game status: " + hostedGame.status);
            bool inProgress = hostedGame.status switch
            {
                GameStatus.InProgress => true,
                _ => false
            };
            if (!inProgress) return;
            
            // Start session
            SessionManager = new DojoSessionManager(this);
            CustomSceneManager.Instance.LoadSession();
            
        }
        
        private void OnSynchronized(List<GameObject> synchronizedModels)
        {
            Debug.Log($"Synchronized {synchronizedModels.Count} models");
        }
        
        public void SpawnEntity(GameObject entity)
        {
            Debug.Log($"Spawned entity: {entity.name}");
        }
        
        public void OnDojoEventReceived(ModelInstance eventMessage)
        {
            Debug.Log($"!!!!!! Received event: {eventMessage.Model.Name}");
        }
        
        // private T GetFieldValue<T>(ModelInstance eventMessage, string fieldName)
        // {
        //     if (eventMessage.Model.Members.TryGetValue(fieldName, out var value))
        //     {
        //         if (value is T typedValue)
        //         {
        //             return typedValue;
        //         }
        //         return (T)Convert.ChangeType(value, typeof(T));
        //     }
        //     throw new KeyNotFoundException($"Field {fieldName} not found in event {eventMessage.Model.Name}");
        // }
        //
        // private void HandleGameCreated(ModelInstance eventMessage)
        // {
        //     var hostPlayer = GetFieldValue<FieldElement>(eventMessage, "host_player");
        //     var status = GetFieldValue<byte>(eventMessage, "status");
        //     Debug.Log($"Game Created by {hostPlayer} with status {status}");
        // }
        //
        // private void HandleGameStarted(ModelInstance eventMessage)
        // {
        //     var hostPlayer = GetFieldValue<FieldElement>(eventMessage, "host_player");
        //     var guestPlayer = GetFieldValue<FieldElement>(eventMessage, "guest_player");
        //     var boardId = GetFieldValue<FieldElement>(eventMessage, "board_id");
        //     Debug.Log($"Game Started between {hostPlayer} and {guestPlayer} on board {boardId}");
        // }
        //
        // private void HandleGameFinished(ModelInstance eventMessage)
        // {
        //     var hostPlayer = GetFieldValue<FieldElement>(eventMessage, "host_player");
        //     var boardId = GetFieldValue<FieldElement>(eventMessage, "board_id");
        //     Debug.Log($"Game Finished for host {hostPlayer} on board {boardId}");
        // }
        //
        // private void HandleMoved(ModelInstance eventMessage)
        // {
        //     var moveId = GetFieldValue<FieldElement>(eventMessage, "move_id");
        //     var player = GetFieldValue<FieldElement>(eventMessage, "player");
        //     var tile = GetFieldValue<byte?>(eventMessage, "tile");
        //     var rotation = GetFieldValue<byte?>(eventMessage, "rotation");
        //     var isJoker = GetFieldValue<bool>(eventMessage, "is_joker");
        //     Debug.Log($"Move made by {player}: Tile={tile}, Rotation={rotation}, IsJoker={isJoker}");
        // }
        //
        // private void HandleInvalidMove(ModelInstance eventMessage)
        // {
        //     var moveId = GetFieldValue<FieldElement>(eventMessage, "move_id");
        //     var player = GetFieldValue<FieldElement>(eventMessage, "player");
        //     Debug.Log($"Invalid move by {player}");
        // }
        //
        // private void HandleGameCanceled(ModelInstance eventMessage)
        // {
        //     var hostPlayer = GetFieldValue<FieldElement>(eventMessage, "host_player");
        //     var status = GetFieldValue<byte>(eventMessage, "status");
        //     Debug.Log($"Game Canceled by {hostPlayer} with status {status}");
        // }
        //
        // private void HandleBoardCreated(ModelInstance eventMessage)
        // {
        //     var boardId = GetFieldValue<FieldElement>(eventMessage, "board_id");
        //     var player1 = GetFieldValue<FieldElement>(eventMessage, "player1");
        //     var player2 = GetFieldValue<FieldElement>(eventMessage, "player2");
        //     Debug.Log($"Board Created: ID={boardId}, Player1={player1}, Player2={player2}");
        // }
        
        void OnDestroy()
        {
            if (WorldManager.synchronizationMaster != null)
            {
                WorldManager.synchronizationMaster.OnEventMessage.RemoveListener(OnDojoEventReceived);
                WorldManager.synchronizationMaster.OnSynchronized.RemoveListener(OnSynchronized);
                WorldManager.synchronizationMaster.OnEntitySpawned.RemoveListener(SpawnEntity);
                WorldManager.synchronizationMaster.OnModelUpdated.RemoveListener(ModelUpdated);
            }
        }
    }
}