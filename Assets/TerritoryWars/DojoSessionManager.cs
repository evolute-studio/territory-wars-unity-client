using System;
using System.Collections;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars
{
    public class DojoSessionManager
    {
        private DojoGameManager _dojoGameManager;

        private Account _localPlayerAccount => _dojoGameManager.LocalBurnerAccount;
        private evolute_duel_Board _localPlayerBoard;
        public string LastMoveIdHex { get; set; }
        public int LastPlayerSide { get; set; }
        
        private Coroutine _checkBoardCoroutine;

        public delegate void OpponentMoveHandler(TileData tile, Vector2Int position, int rotation);

        public event OpponentMoveHandler OnOpponentMoveReceived;

        public evolute_duel_Board LocalPlayerBoard
        {
            get
            {
                if (_localPlayerBoard == null)
                {
                    _localPlayerBoard = GetLocalPlayerBoard();
                }

                return _localPlayerBoard;
            }
            private set => _localPlayerBoard = value;
        }

        public DojoSessionManager(DojoGameManager dojoGameManager)
        {
            _dojoGameManager = dojoGameManager;
            dojoGameManager.WorldManager.synchronizationMaster.OnModelUpdated.AddListener(OnModelUpdated);
            //dojoGameManager.WorldManager.synchronizationMaster.OnEntitySpawned.AddListener(OnEntitySpawned);
        }
        
        private void OnModelUpdated(ModelInstance modelInstance)
        {
            if(_dojoGameManager.IsTargetModel(modelInstance, nameof(evolute_duel_Board)))
            {
                CustomLogger.LogImportant($"Model {nameof(evolute_duel_Board)} via OnModelUpdated");
                CheckBoardUpdate();
            }
        }
        
        private void OnEntitySpawned(GameObject newEntity)
        {
            if(!newEntity.TryGetComponent(out evolute_duel_Move move)) return;
        }

        public void CheckNewMove(evolute_duel_Move move)
        {
            if (move == null) return;
            var moveData = OnChainMoveDataConverter.GetMoveData(move);
            CustomLogger.LogModelUpdate($"[New Move]: Account {moveData.Item1} made a move at {moveData.Item2}, {moveData.Item3}. Rotation: {moveData.Item4}");
            if (moveData.Item1 != null)
            {
                LastPlayerSide = moveData.Item1 switch
                {
                    PlayerSide.Blue => 0,
                    PlayerSide.Red => 1,
                };
                OnOpponentMoveReceived?.Invoke(moveData.Item2, moveData.Item4, moveData.Item3);
            }
            
        }

        public void StartBoardChecking()
        {
            _checkBoardCoroutine = _dojoGameManager.StartCoroutine(CheckBoardCoroutine(0.5f));
        }
        
        public void StopBoardChecking()
        {
            if (_checkBoardCoroutine != null)
            {
                _dojoGameManager.StopCoroutine(_checkBoardCoroutine);
            }
        }

        private IEnumerator CheckBoardCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            CheckBoardUpdate();
        }
        

        public void CheckBoardUpdate()
        {
            CustomLogger.LogImportant($"Checking board update");
            if(LocalPlayerBoard == null || LocalPlayerBoard.last_move_id == null) return;
            CustomLogger.LogImportant($"Board is not null");
            string newMove = LocalPlayerBoard.last_move_id switch
            {
                Option<FieldElement>.Some moveId => moveId.value.Hex(),
                Option<FieldElement>.None => null
            };
            CustomLogger.LogImportant($"New move equals LastMoveIdHex: {newMove == LastMoveIdHex}. LastMoveIdHex: {LastMoveIdHex} NewMove: {newMove}");
            if (newMove != LastMoveIdHex && LocalPlayerBoard != null)
            {
                evolute_duel_Move moveModel = GetMoveModelById(LocalPlayerBoard.last_move_id);
                CustomLogger.LogImportant($"Move model is null: {moveModel == null}");
                if (moveModel == null)
                {
                    DojoGameManager.Instance.TryAgain(CheckBoardUpdate, 1f);
                }
                var moveData = OnChainMoveDataConverter.GetMoveData(moveModel);
                LastPlayerSide = moveData.Item1 switch
                {
                    PlayerSide.Blue => 0,
                    PlayerSide.Red => 1,
                };
                LastMoveIdHex = newMove;
                Moved(moveData);
            }
        }

        public void Moved((PlayerSide playerSide, TileData tile, int rotation, Vector2Int position, bool isJoker) moveData)
        {
            CustomLogger.LogModelUpdate($"[New Move]: Account {moveData.playerSide} made a move at {moveData.position.x}, {moveData.position.y}. Rotation: {moveData.rotation}");
            OnOpponentMoveReceived?.Invoke(moveData.tile, moveData.position, moveData.rotation);
        }
        
        
        
        
        
        public evolute_duel_Board GetLocalPlayerBoard()
        {
            return GetBoard(_dojoGameManager.LocalBurnerAccount.Address);
        }

        private evolute_duel_Board GetBoard(FieldElement player)
        {
            GameObject[] boardsGO = _dojoGameManager.WorldManager.Entities<evolute_duel_Board>();
            foreach (var boardGO in boardsGO)
            {
                if (boardGO.TryGetComponent(out evolute_duel_Board board))
                {
                    //public (FieldElement, PlayerSide, byte, bool) player1;
                    if (board.player1.Item1.Hex() == player.Hex() || board.player2.Item1.Hex() == player.Hex())
                    {
                        return board;
                    }
                }
            }
            return null;
        }
        
        public TileData GetTopTile()
        {
            if (LocalPlayerBoard == null) return null;
            return new TileData(OnChainBoardDataConverter.GetTopTile(LocalPlayerBoard.top_tile));
        }

        public async void MakeMove(TileData data, int x, int y)
        {
            try
            {
                Account account = _dojoGameManager.LocalBurnerAccount;
                Option<byte> jokerTile = new Option<byte>.None();
                byte rotation = (byte) data.rotationIndex;
                byte col = (byte) (x - 1);
                byte row = (byte) (y - 1);
                var txHash = await _dojoGameManager.GameSystem.make_move(account, jokerTile, rotation, col, row);
                CustomLogger.LogModelUpdate($"[Make Move]: Account {account.Address.Hex()} made a move at {x}, {y}. Rotation: {rotation}");
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Error making move: {e.Message}");
            }
        }

        private evolute_duel_Move GetMoveModelById(Option<FieldElement> move_id)
        {
            GameObject[] movesGO = _dojoGameManager.WorldManager.Entities<evolute_duel_Move>();
            foreach (var moveGO in movesGO)
            {
                if (moveGO.TryGetComponent(out evolute_duel_Move move))
                {
                    string moveId = move_id switch
                    {
                        Option<FieldElement>.Some id => id.value.Hex(),
                        Option<FieldElement>.None => null
                    };
                    if (moveId == null) continue;
                    if (move.id.Hex() == moveId)
                    {
                        return move;
                    }
                }
            }
            return null;
            
        }
        
        
    }
}