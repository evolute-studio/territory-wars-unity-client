using System;
using System.Collections;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
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
            Debug.Log("DojoSessionManager initialized. Session started");
            _dojoGameManager = dojoGameManager;
            _dojoGameManager.WorldManager.synchronizationMaster.OnModelUpdated.AddListener(ModelUpdated);
        }

        public void ModelUpdated(ModelInstance modelInstance)
        {
            
        }

        public void CheckBoardUpdate()
        {
            if(LocalPlayerBoard == null || LocalPlayerBoard.last_move_id == null) return;
            
            string newMove = LocalPlayerBoard.last_move_id switch
            {
                Option<FieldElement>.Some moveId => moveId.value.Hex(),
                Option<FieldElement>.None => null
            };
            
            if (newMove != LastMoveIdHex || LocalPlayerBoard != null)
            {
                LastMoveIdHex = newMove;
                Debug.Log($"New move detected: {LastMoveIdHex}");
                evolute_duel_Move moveModel = GetMoveModelById(LocalPlayerBoard.last_move_id);
                if (moveModel == null) return;
                var moveData = OnChainMoveDataConverter.GetMoveData(moveModel);
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
            else
            {
                _dojoGameManager.TryAgain(CheckBoardUpdate, 1f);
            }
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
                byte col = (byte) x;
                byte row = (byte) y;
                var txHash = await _dojoGameManager.GameSystem.make_move(account, jokerTile, rotation, col, row);
                Debug.Log($"[Move]: Account {account.Address.Hex()} made a move at {x}, {y}. Transaction hash: {txHash.Hex()}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
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