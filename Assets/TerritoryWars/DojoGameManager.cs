using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using bottlenoselabs.C2CS.Runtime;
using Dojo;
using Dojo.Starknet;
using UnityEngine;

// fix to use Records in Unity ref. https://stackoverflow.com/a/73100830
using System.ComponentModel;
using System.Threading.Tasks;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine.Events;
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

        public WorldManagerData dojoConfig;
        [SerializeField] GameManagerData gameManagerData;
        
        public Game GameSystem;
        public Player_profile_actions PlayerProfileSystem;

        public BurnerManager burnerManager;

        public JsonRpcClient provider;
        public Account masterAccount;
        
        public Account LocalBurnerAccount { get; private set; }

        public bool IsLocalPlayer;
        
        public DojoSessionManager SessionManager;
        
        public UnityEvent OnLocalPlayerSet = new UnityEvent();
        
        public bool Synced { get; private set; }


        public void Initialize()
        {
            provider = new JsonRpcClient(dojoConfig.rpcUrl);
            masterAccount = new Account(provider, new SigningKey(gameManagerData.masterPrivateKey),
                new FieldElement(gameManagerData.masterAddress));
            
            
            Invoke(nameof(createBurners), 1f);
            
            //WorldManager.synchronizationMaster.OnModelUpdated.AddListener(ModelUpdated);
            //SimpleAccountCreation(3);
            
        }

        private void createBurners()
        {
            burnerManager = new BurnerManager(provider, masterAccount);
            
            
            WorldManager.synchronizationMaster.OnEventMessage.AddListener(OnDojoEventReceived);
            WorldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
            WorldManager.synchronizationMaster.OnSynchronized.AddListener(OnSynchronized);
            WorldManager.synchronizationMaster.OnEntitySpawned.AddListener(SpawnEntity);
            
            TryCreateAccount(3, false);
            //AfterAccountCreation();
        }



        private void TurnOffLoading(List<GameObject> list)
        {
            WorldManager.synchronizationMaster.OnSynchronized.RemoveListener(TurnOffLoading);
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
            AfterAccountCreation();
            //MenuUIController.Instance._namePanelController.Initialize();

        }
        
        public void AfterAccountCreation()
        {
            string currentBoardId = SimpleStorage.GetCurrentBoardId();
            CustomLogger.LogInfo($"Current board id: {currentBoardId}");
            if (!String.IsNullOrEmpty(currentBoardId))
            {
                SessionManager = new DojoSessionManager(this);
                evolute_duel_Board board = SessionManager.GetBoard(LocalBurnerAccount.Address.Hex(), true);
                CustomLogger.LogInfo($"Board: {board}");
                if (board == null)
                {
                    return;
                }
                var isFinished = board.game_state switch
                {
                    GameState.Finished => true,
                    _ => false
                };
                CustomLogger.LogInfo($"IsFinished: {isFinished}");
                if (isFinished)
                {
                    SimpleStorage.ClearCurrentBoardId();
                    CustomSceneManager.Instance.LoadLobby();
                    return;
                }

                CustomSceneManager.Instance.LoadSession();
            } 
            else
            {
                CustomSceneManager.Instance.LoadLobby();
            }

        }
        
        private async void SimpleAccountCreation(int attempts)
        {
            try
            {
                for (int i = 0; i < attempts; i++)
                {
                    LocalBurnerAccount = await burnerManager.DeployBurner();
                    if (LocalBurnerAccount != null)
                    {
                        OnLocalPlayerSet?.Invoke();
                        CustomLogger.LogInfo($"Burner account created. Attempt: {i}. Address: {LocalBurnerAccount.Address}");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Failed to create burner account. {e}");
            }
        }
        
        private async void TryCreateAccount(int attempts, bool createNew)
        {
            try
            {
                for (int i = 0; i < attempts; i++)
                {
                    CustomLogger.LogInfo($"Creating burner account. Attempt: {i}");
                    if (await CreateAccount(createNew))
                    {
                        CustomLogger.LogInfo($"Burner account created. Attempt: {i}. Address: {LocalBurnerAccount.Address}");
                        OnLocalPlayerSet?.Invoke();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Failed to create burner account. {e}");
            }
        }
        
        public void TryCreateNewAccount()
        {
            TryCreateAccount(6, true);
        }
        
        
        private async Task<bool> CreateAccount(bool createNew)
        {
            try
            {
                if (createNew)
                {
                    LocalBurnerAccount = await burnerManager.DeployBurner();
                }
                else
                {
                    if (burnerManager.Burners.Count == 0)
                    {
                        CustomLogger.LogWarning("Burner account not found. Creating new account.");
                        LocalBurnerAccount = await burnerManager.DeployBurner();
                    }
                    else
                    {
                        CustomLogger.LogInfo("Burner account found. Using last account: " + burnerManager.CurrentBurner.Address.Hex());
                        //use last burner account
                        LocalBurnerAccount = burnerManager.CurrentBurner;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public GameObject[] GetGames() => WorldManager.Entities<evolute_duel_Game>();
        public GameObject[] GetSnapshots() => WorldManager.Entities<evolute_duel_Snapshot>();
        
        public evolute_duel_Snapshot GetSnapshot(FieldElement snapshotId)
        {
            if (snapshotId == null) return null;
            GameObject[] snapshots = GetSnapshots();
            foreach (var snapshot in snapshots)
            {
                if (snapshot.TryGetComponent(out evolute_duel_Snapshot snapshotModel))
                {
                    if (snapshotModel.snapshot_id.Hex() == snapshotId.Hex())
                    {
                        return snapshotModel;
                    }
                }
            }
            return null;
        }
        
        public async void CreateGame()
        {
            try
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(true, CancelGame, CustomSceneManager.Instance.LoadingScreen.loadingTextPrefix);
                var txHash = await GameSystem.create_game(LocalBurnerAccount);
                CustomLogger.LogInfo($"Create Game: {txHash.Hex()}");
            }
            catch (Exception e)
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(false);
                CustomLogger.LogError($"Failed to create game. {e}");
            }
        }
        
        public async void CreateGameFromSnapshot(FieldElement snapshotId)
        {
            try
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(true, CancelGame, CustomSceneManager.Instance.LoadingScreen.loadingTextPrefix);
                var txHash = await GameSystem.create_game_from_snapshot(LocalBurnerAccount, snapshotId);
                CustomLogger.LogInfo($"Create Game from Snapshot: {txHash.Hex()}");
            }
            catch (Exception e)
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(false);
                CustomLogger.LogError($"Failed to create game from snapshot. {e}");
            }
        }
        
        public async void JoinGame(FieldElement hostPlayer)
        {
            try
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(true, CancelGame, CustomSceneManager.Instance.LoadingScreen.loadingTextPrefix);
                var txHash = await GameSystem.join_game(LocalBurnerAccount, hostPlayer);
                CustomLogger.LogInfo($"Join Game: {txHash.Hex()}");
                SessionManager = new DojoSessionManager(this);
                //CustomSceneManager.Instance.LoadSession();
                
            }
            catch (Exception e)
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(false);
                CustomLogger.LogError($"Failed to join game. {e}");
            }
        }
        
        public async void CancelGame()
        {
            try
            {
                var txHash = await GameSystem.cancel_game(LocalBurnerAccount);
                CustomLogger.LogInfo($"Cancel Game: {txHash.Hex()}");
                CustomSceneManager.Instance.LoadingScreen.SetActive(false);
                if(CustomSceneManager.Instance.CurrentScene != CustomSceneManager.Instance.Menu)
                {
                    CustomSceneManager.Instance.LoadLobby();
                }
            }
            catch (Exception e)
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(false);
                CustomLogger.LogError($"Failed to cancel game. {e}");
                if(CustomSceneManager.Instance.CurrentScene != CustomSceneManager.Instance.Menu)
                {
                    CustomSceneManager.Instance.LoadLobby();
                }
            }
        }
        
        public async void ChangePlayerSkin(int skinId)
        {
            try
            {
                CustomLogger.LogInfo("Set player skin");
                var txHash = await PlayerProfileSystem.change_skin(LocalBurnerAccount, (byte)skinId);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("ContractNotFound"))
                {
                    CustomLogger.LogError("The contract was not found. Maybe a problem in creating an account");
                    SimpleAccountCreation(3);
                }
                else
                {
                    CustomLogger.LogError($"Failed to set player skin. {e}");
                }
            }
        }
        
        private void ModelUpdated(ModelInstance modelInstance)
        {
            if (modelInstance == null || modelInstance.transform == null) return;
            
            if (IsTargetModel(modelInstance, nameof(evolute_duel_Game)))
            {
                
            }
        }
        
        private void OnEventMessage(ModelInstance modelInstance)
        {
            switch (modelInstance)
            {
                case evolute_duel_PlayerUsernameChanged playerUsernameChanged:
                    PlayerUsernameChanged(playerUsernameChanged);
                    break;
                case evolute_duel_GameCreateFailed gameCreateFailed:
                    GameCreateFailed(gameCreateFailed);
                    break;
            }
        }
        
        private void PlayerUsernameChanged(evolute_duel_PlayerUsernameChanged eventMessage)
        {
            CustomLogger.LogWarning("Burner account address:" + LocalBurnerAccount.Address.Hex());
            CustomLogger.LogWarning("Real caller address from event: " + eventMessage.player_id.Hex());
            if(LocalBurnerAccount == null || LocalBurnerAccount.Address.Hex() != eventMessage.player_id.Hex()) return;
            MenuUIController.Instance._namePanelController.SetName(CairoFieldsConverter.GetStringFromFieldElement(eventMessage.new_username));
        }
        
        private void GameCreateFailed(evolute_duel_GameCreateFailed eventMessage)
        {
            string hostPlayer = eventMessage.host_player.Hex();
            if (LocalBurnerAccount.Address.Hex() != hostPlayer) return;
            CancelGame();
            Invoke(nameof(CreateGame), 1f);
        }
        
        
        public void OnDojoEventReceived(ModelInstance eventMessage)
        {
            CustomLogger.LogImportant($"Received event: {eventMessage.Model.Name}");
            if (IsTargetModel(eventMessage, nameof(evolute_duel_GameStarted)))
            {
                CheckStartSession(eventMessage);
            }
        }

        private void CheckStartSession(ModelInstance eventMessage)
        {
            evolute_duel_GameStarted gameStarted = eventMessage as evolute_duel_GameStarted;
            if (gameStarted == null)
            {
                CustomLogger.LogWarning($"Event {nameof(evolute_duel_Game)} is null");
                return;
            }
            CustomLogger.LogInfo($"Check start session. " +
                                 $"\nLocalPlayerIsHost: {gameStarted.host_player.Hex() == LocalBurnerAccount.Address.Hex()}" +
                                 $"\nLocalPlayerIsGuest: {gameStarted.guest_player.Hex() == LocalBurnerAccount.Address.Hex()}" +
                                 $"\nEventHostPlayerAddress: {gameStarted.host_player.Hex()}" +
                                 $"\nEventGuestPlayerAddress: {gameStarted.guest_player.Hex()}" +
                                 $"\nLocalPlayerAddress: {LocalBurnerAccount.Address.Hex()}");
            if (gameStarted.host_player.Hex() == LocalBurnerAccount.Address.Hex() ||
                gameStarted.guest_player.Hex() == LocalBurnerAccount.Address.Hex())
            {
                CustomLogger.LogInfo("Start session");
                // Start session
                SessionManager = new DojoSessionManager(this);
                CustomSceneManager.Instance.LoadSession();
            }
        }
        
        public bool IsTargetModel(ModelInstance modelInstance, string targetModelName)
        {
            string modelInstanceName = modelInstance.ToString();
            if (modelInstanceName.Contains(targetModelName))
            {
                return true;
            }
            return false;
        }

        public evolute_duel_Player GetPlayerData(string address)
        {
            GameObject[] playerModelsGO = WorldManager.Entities<evolute_duel_Player>();
            foreach (var playerModelGO in playerModelsGO)
            {
                if (playerModelGO.TryGetComponent(out evolute_duel_Player playerModel))
                {
                    if (playerModel.player_id.Hex() == address)
                    {
                        return playerModel;
                    }
                }
            }
            return null;
        }


        public evolute_duel_Move GetMove(FieldElement moveId)
        {
            GameObject[] moveModelsGO = WorldManager.Entities<evolute_duel_Move>();
            foreach (var moveModelGO in moveModelsGO)
            {
                if (moveModelGO.TryGetComponent(out evolute_duel_Move moveModel))
                {
                    if (moveModel.id.Hex() == moveId.Hex())
                    {
                        return moveModel;
                    }
                }
            }
            return null;
        }
        
        public List<evolute_duel_Move> GetMoves(List<evolute_duel_Move> moves, GameObject[] allMoveGameObjects = null)
        {
            CustomLogger.LogInfo($"GetMoves: {moves.Count}");
            if (allMoveGameObjects == null)
            {
                allMoveGameObjects = WorldManager.Entities<evolute_duel_Move>();
            }
            
            evolute_duel_Move currentMove = moves.First();
            FieldElement previousMoveId = currentMove.prev_move_id switch
            {
                Option<FieldElement>.Some some => some.value,
                _ => null
            };
            if (previousMoveId == null)
            {
                return moves;
            }
            
            foreach (var moveGO in allMoveGameObjects)
            {
                if (moveGO.TryGetComponent(out evolute_duel_Move move))
                {
                    if (move.id.Hex() == previousMoveId.Hex())
                    {
                        moves.Insert(0, move);
                        return GetMoves(moves, allMoveGameObjects);
                    }
                }
            }

            return moves;
        }
        
        public evolute_duel_Player GetLocalPlayerData()
        {
            return GetPlayerData(LocalBurnerAccount.Address.Hex());
        }
        
        public async void SetPlayerName(string playerName)
        {
            try
            {
                FieldElement username = CairoFieldsConverter.GetFieldElementFromString(playerName);
                var txHash = await PlayerProfileSystem.change_username(LocalBurnerAccount, username);
                CustomLogger.LogInfo($"Set Player Name: {playerName + "; tx" + txHash.Hex()}");
                CustomLogger.LogInfo($"Set Player Name, signer address: {LocalBurnerAccount.Address.Hex()}");
                CustomLogger.LogInfo($"Set Player Name, signer pub key: {LocalBurnerAccount.Signer.PublicKey.Inner.Hex()}");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("ContractNotFound"))
                {
                    CustomLogger.LogError("The contract was not found. Maybe a problem in creating an account");
                    SimpleAccountCreation(3);
                }
                else
                {
                    CustomLogger.LogError($"Failed to set player name. {e}");
                }
                
            }
        }
        
        public async void SetPlayerName(FieldElement playerName)
        {
            try
            {
                var txHash = await PlayerProfileSystem.change_username(LocalBurnerAccount, playerName);
                CustomLogger.LogInfo($"Set Player Name: {playerName + "; tx" + txHash.Hex()}");
                CustomLogger.LogInfo($"Set Player Name, burner address: {LocalBurnerAccount.Address.Hex()}");
                CustomLogger.LogInfo($"Set Player Name, signer pub key: {LocalBurnerAccount.Signer.PublicKey.Inner.Hex()}");
            }
            catch (Exception e)
            {
                if (e.Message.Contains("ContractNotFound"))
                {
                    CustomLogger.LogError("The contract was not found. Maybe a problem in creating an account");
                    SimpleAccountCreation(3);
                }
                else
                {
                    CustomLogger.LogError($"Failed to set player name. {e}");
                }
            }
        }
        
        private void OnSynchronized(List<GameObject> synchronizedModels)
        {
            Synced = true;
            CustomLogger.LogInfo($"Synchronized {synchronizedModels.Count} models");
        }
        
        public void SpawnEntity(GameObject entity)
        {
            CustomLogger.LogInfo($"Spawned entity: {entity.name}");
        }
        
        
        public evolute_duel_Player GetPlayerProfileByAddress(string address)
        {
            GameObject[] playerProfiles = WorldManager.Entities<evolute_duel_Player>();
            foreach (var playerProfile in playerProfiles)
            {
                if (playerProfile.TryGetComponent(out evolute_duel_Player player))
                {
                    if (player.player_id.Hex() == address)
                    {
                        return player;
                    }
                }
            }
            return null;
        }
        
        
        public void TryAgain(Action action, float delay)
        {
            StartCoroutine(TryAgainCoroutine(action, delay));
        }
        
        private IEnumerator TryAgainCoroutine(Action action, float delay){
            yield return new WaitForSeconds(delay);
            action();
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
                //WorldManager.synchronizationMaster.OnModelUpdated.RemoveListener(ModelUpdated);
            }
        }
    }
}