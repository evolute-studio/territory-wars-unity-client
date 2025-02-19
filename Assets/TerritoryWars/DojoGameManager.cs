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

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}

namespace TerritoryWars
{
    public class DojoGameManager : MonoBehaviour
    {
        [SerializeField] WorldManager worldManager;

        [SerializeField] WorldManagerData dojoConfig;
        [SerializeField] GameManagerData gameManagerData;

        public BurnerManager burnerManager;

        public JsonRpcClient provider;
        public Account masterAccount;
        
        public Account LocalBurnerAccount { get; private set; }


        void Start()
        {
            provider = new JsonRpcClient(dojoConfig.rpcUrl);
            masterAccount = new Account(provider, new SigningKey(gameManagerData.masterPrivateKey),
                new FieldElement(gameManagerData.masterAddress));
            burnerManager = new BurnerManager(provider, masterAccount);

            worldManager.synchronizationMaster.OnEventMessage.AddListener(OnDojoEventReceived);
            worldManager.synchronizationMaster.OnSynchronized.AddListener(OnSynchronized);
            worldManager.synchronizationMaster.OnEntitySpawned.AddListener(SpawnEntity);
            worldManager.synchronizationMaster.OnModelUpdated.AddListener(modelInstance =>
            {
                 Debug.Log($"Model updated: {modelInstance.Model.Name}");
            });
            
            CreateAccount();
        }
        
        private async void CreateAccount()
        {
            try
            {
                LocalBurnerAccount = await burnerManager.DeployBurner();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
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
            Debug.Log($"Received event: {eventMessage.Model.Name}");
            switch (eventMessage.Model.Name)
            {
                case "GameCreated":
                    HandleGameCreated(eventMessage);
                    break;
                case "GameStarted":
                    HandleGameStarted(eventMessage);
                    break;
                case "GameFinished":
                    HandleGameFinished(eventMessage);
                    break;
                case "Moved":
                    HandleMoved(eventMessage);
                    break;
                case "InvalidMove":
                    HandleInvalidMove(eventMessage);
                    break;
                case "GameCanceled":
                    HandleGameCanceled(eventMessage);
                    break;
                case "BoardCreated":
                    HandleBoardCreated(eventMessage);
                    break;
            }
        }
        
        private T GetFieldValue<T>(ModelInstance eventMessage, string fieldName)
        {
            if (eventMessage.Model.Members.TryGetValue(fieldName, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
            throw new KeyNotFoundException($"Field {fieldName} not found in event {eventMessage.Model.Name}");
        }
        
        private void HandleGameCreated(ModelInstance eventMessage)
        {
            var hostPlayer = GetFieldValue<FieldElement>(eventMessage, "host_player");
            var status = GetFieldValue<byte>(eventMessage, "status");
            Debug.Log($"Game Created by {hostPlayer} with status {status}");
        }
        
        private void HandleGameStarted(ModelInstance eventMessage)
        {
            var hostPlayer = GetFieldValue<FieldElement>(eventMessage, "host_player");
            var guestPlayer = GetFieldValue<FieldElement>(eventMessage, "guest_player");
            var boardId = GetFieldValue<FieldElement>(eventMessage, "board_id");
            Debug.Log($"Game Started between {hostPlayer} and {guestPlayer} on board {boardId}");
        }
        
        private void HandleGameFinished(ModelInstance eventMessage)
        {
            var hostPlayer = GetFieldValue<FieldElement>(eventMessage, "host_player");
            var boardId = GetFieldValue<FieldElement>(eventMessage, "board_id");
            Debug.Log($"Game Finished for host {hostPlayer} on board {boardId}");
        }
        
        private void HandleMoved(ModelInstance eventMessage)
        {
            var moveId = GetFieldValue<FieldElement>(eventMessage, "move_id");
            var player = GetFieldValue<FieldElement>(eventMessage, "player");
            var tile = GetFieldValue<byte?>(eventMessage, "tile");
            var rotation = GetFieldValue<byte?>(eventMessage, "rotation");
            var isJoker = GetFieldValue<bool>(eventMessage, "is_joker");
            Debug.Log($"Move made by {player}: Tile={tile}, Rotation={rotation}, IsJoker={isJoker}");
        }
        
        private void HandleInvalidMove(ModelInstance eventMessage)
        {
            var moveId = GetFieldValue<FieldElement>(eventMessage, "move_id");
            var player = GetFieldValue<FieldElement>(eventMessage, "player");
            Debug.Log($"Invalid move by {player}");
        }
        
        private void HandleGameCanceled(ModelInstance eventMessage)
        {
            var hostPlayer = GetFieldValue<FieldElement>(eventMessage, "host_player");
            var status = GetFieldValue<byte>(eventMessage, "status");
            Debug.Log($"Game Canceled by {hostPlayer} with status {status}");
        }
        
        private void HandleBoardCreated(ModelInstance eventMessage)
        {
            var boardId = GetFieldValue<FieldElement>(eventMessage, "board_id");
            var player1 = GetFieldValue<FieldElement>(eventMessage, "player1");
            var player2 = GetFieldValue<FieldElement>(eventMessage, "player2");
            Debug.Log($"Board Created: ID={boardId}, Player1={player1}, Player2={player2}");
        }
        
        void OnDestroy()
        {
            if (worldManager.synchronizationMaster != null)
            {
                worldManager.synchronizationMaster.OnEventMessage.RemoveListener(OnDojoEventReceived);
            }
        }
    }
}