using System;
using System.Collections;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;

namespace TerritoryWars
{
    public class DojoSessionManager
    {
        private DojoGameManager _dojoGameManager;

        private Account _localPlayerAccount => _dojoGameManager.LocalBurnerAccount;
        private evolute_duel_Board _localPlayerBoard;
        private int _moveCount = 0;
        private int _snapshotTurn = 0;
        //public string LastMoveIdHex { get; set; }
        //public int LastPlayerSide { get; set; }

        public delegate void MoveHandler(string playerAddress, TileData tile, Vector2Int position, int rotation);

        public event MoveHandler OnMoveReceived;
        public delegate void SkipMoveHandler(string address);
        public event SkipMoveHandler OnSkipMoveReceived;

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
            dojoGameManager.WorldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
        }
        
        private void OnModelUpdated(ModelInstance modelInstance)
        {
            if(_dojoGameManager.IsTargetModel(modelInstance, nameof(evolute_duel_Board)))
            {
                //CustomLogger.LogImportant($"Model {nameof(evolute_duel_Board)} via OnModelUpdated");
            }
        }
        
        private void OnEventMessage(ModelInstance modelInstance)
        {
            switch (modelInstance)
            {
                case evolute_duel_Moved moved:
                    Moved(moved);
                    break;
                case evolute_duel_InvalidMove invalidMove:
                    InvalidMove(invalidMove);
                    break;
                case evolute_duel_Skiped skipped:
                    Skipped(skipped);
                    break;
                case evolute_duel_BoardUpdated boardUpdated:
                    BoardUpdated(boardUpdated);
                    break;
                case evolute_duel_GameFinished gameFinished:
                    GameFinished(gameFinished.board_id);
                    break;
                case evolute_duel_GameIsAlreadyFinished gameIsAlreadyFinished:
                    GameFinished(gameIsAlreadyFinished.board_id);
                    break;
            }
            if(_dojoGameManager.IsTargetModel(modelInstance, nameof(evolute_duel_Moved)))
            {
            }
            else if(_dojoGameManager.IsTargetModel(modelInstance, nameof(evolute_duel_Moved)))
            {
            }
        }

        private void Moved(evolute_duel_Moved eventModel)
        {
            string player = eventModel.player.Hex();
            if (player != SessionManager.Instance.LocalPlayer.Address.Hex() && player != SessionManager.Instance.RemotePlayer.Address.Hex()) return;
            
            _moveCount++;
            string move_id = eventModel.move_id.Hex();
            string prev_move_id = eventModel.prev_move_id switch
            {
                Option<FieldElement>.Some id => id.value.Hex(),
                Option<FieldElement>.None => null
            };
            TileData tile = eventModel.tile is Option<byte>.Some some ? new TileData(OnChainBoardDataConverter.TileTypes[some.value]) : null;
            int rotation = (eventModel.rotation + 3) % 4;
            Vector2Int position = new Vector2Int(eventModel.col, eventModel.row);
            bool isJoker = eventModel.is_joker;
            string board_id = eventModel.board_id.Hex();
            
            CustomLogger.LogEvent($"[Moved] | Player: {player} | MoveId: {move_id} | PrevMoveId: {prev_move_id} | Tile: {tile} | Rotation: {rotation} | Position: {position} | IsJoker: {isJoker} | BoardId: {board_id}");
            OnMoveReceived?.Invoke(player, tile, position, rotation);
        }
        
        private void InvalidMove(evolute_duel_InvalidMove eventModel)
        {
            string move_id = eventModel.move_id.Hex();
            string player = eventModel.player.Hex();
            
            CustomLogger.LogError($"[InvalidMove] | Player: {player} | MoveId: {move_id}");
        }
        
        private void Skipped(evolute_duel_Skiped eventModel)
        {
            string player = eventModel.player.Hex();
            CustomLogger.LogEvent($"[Skipped] | Player: {player}");
            OnSkipMoveReceived?.Invoke(player);
        }
        
        private void BoardUpdated(evolute_duel_BoardUpdated eventModel)
        {
            string board_id = eventModel.board_id.Hex();
            CustomLogger.LogEvent($"[BoardUpdated] | BoardId: {board_id}");
            int cityScoreBlue = eventModel.blue_score.Item1;
            int cartScoreBlue = eventModel.blue_score.Item2;
            int cityScoreRed = eventModel.red_score.Item1;
            int cartScoreRed = eventModel.red_score.Item2;
            GameUI.Instance.SessionUI.SetCityScore(0, cityScoreBlue);
            GameUI.Instance.SessionUI.SetCityScore(1, cityScoreRed);
            GameUI.Instance.SessionUI.SetRoadScore(0, cartScoreBlue);
            GameUI.Instance.SessionUI.SetRoadScore(1, cartScoreRed);
        }
        
        private void GameFinished(FieldElement board_id)
        {
            evolute_duel_Board board = GetLocalPlayerBoard(true);
            if (board.id.Hex() == board_id.Hex())
            {
                CustomLogger.LogEvent($"[GameFinished] | BoardId: {board_id.Hex()}");
                GameUI.Instance.ShowResultPopUp();
                CreateSnapshot();
            }
        }
        
        // public void CheckBoardUpdate()
        // {
        //     CustomLogger.LogImportant($"Checking board update");
        //     if(LocalPlayerBoard == null || LocalPlayerBoard.last_move_id == null) return;
        //     CustomLogger.LogImportant($"Board is not null");
        //     string newMove = LocalPlayerBoard.last_move_id switch
        //     {
        //         Option<FieldElement>.Some moveId => moveId.value.Hex(),
        //         Option<FieldElement>.None => null
        //     };
        //     CustomLogger.LogImportant($"New move equals LastMoveIdHex: {newMove == LastMoveIdHex}. LastMoveIdHex: {LastMoveIdHex} NewMove: {newMove}");
        //     if (newMove != LastMoveIdHex && LocalPlayerBoard != null)
        //     {
        //         evolute_duel_Move moveModel = GetMoveModelById(LocalPlayerBoard.last_move_id);
        //         CustomLogger.LogImportant($"Move model is null: {moveModel == null}");
        //         if (moveModel == null)
        //         {
        //             DojoGameManager.Instance.TryAgain(CheckBoardUpdate, 1f);
        //         }
        //         var moveData = OnChainMoveDataConverter.GetMoveData(moveModel);
        //         LastPlayerSide = moveData.Item1 switch
        //         {
        //             PlayerSide.Blue => 0,
        //             PlayerSide.Red => 1,
        //         };
        //         if(LastPlayerSide == SessionManager.Instance.LocalPlayer.LocalId) return;
        //         LastMoveIdHex = newMove;
        //         Moved(moveData);
        //     }
        // }
        //
        // public void Moved((PlayerSide playerSide, TileData tile, int rotation, Vector2Int position, bool isJoker) moveData)
        // {
        //     CustomLogger.LogEvent($"[New Move]: Account {moveData.playerSide} made a move at {moveData.position.x}, {moveData.position.y}. Rotation: {moveData.rotation}");
        //     OnMoveReceived?.Invoke(moveData.tile, moveData.position, moveData.rotation);
        // }
        
        
        
        
        
        public evolute_duel_Board GetLocalPlayerBoard(bool isFinished = false)
        {
            return GetBoard(_dojoGameManager.LocalBurnerAccount.Address, isFinished);
        }

        private evolute_duel_Board GetBoard(FieldElement player, bool isFinished = false)
        {
            GameObject[] boardsGO = _dojoGameManager.WorldManager.Entities<evolute_duel_Board>();
            foreach (var boardGO in boardsGO)
            {
                if (boardGO.TryGetComponent(out evolute_duel_Board board))
                {
                    if (board.game_state is GameState.Finished && !isFinished) continue;
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

        public async void MakeMove(TileData data, int x, int y, bool isJoker)
        {
            Account account = _dojoGameManager.LocalBurnerAccount;
            var tileConfig = OnChainBoardDataConverter.GetTypeAndRotation(data.id);
            Option<byte> jokerTile = isJoker ? new Option<byte>.Some(tileConfig.Item1) : new Option<byte>.None();
            byte rotation = (byte)((tileConfig.Item2 + 1) % 4);
            byte col = (byte) (x - 1);
            byte row = (byte) (y - 1);
            try
            {
                var txHash = await _dojoGameManager.GameSystem.make_move(account, jokerTile, rotation, col, row);
                CustomLogger.LogEvent($"[Make Move]: Hash {txHash} Account {account.Address.Hex()} made a move at {x}, {y}. Rotation: {rotation}");
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Error making move: {e.Message}. " +
                                      $"\n| account: {account.Address.Hex()} |" +
                                      $"jokerTile: {jokerTile} |" +
                                      $"rotation: {rotation} |" +
                                      $"col: {col} |" +
                                      $"row: {row} |" +
                                      $"tile config: {data}");
            }
        }
        
        public void SetSnapshotTurn()
        {
            _snapshotTurn = _moveCount;
        }
        
        public void CreateSnapshot()
        {
            if (_snapshotTurn == 0) return;
            
            try
            {
                var txHash = _dojoGameManager.GameSystem.create_snapshot(_localPlayerAccount, LocalPlayerBoard.id, (byte)_snapshotTurn);
                CustomLogger.LogEvent($"[Create Snapshot]: Hash {txHash} Account {_localPlayerAccount.Address.Hex()} created a snapshot");
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Error creating snapshot: {e.Message}. " +
                                      $"\n| account: {_localPlayerAccount.Address.Hex()} |" +
                                      $"boardId: {LocalPlayerBoard.id} |" +
                                      $"snapshotTurn: {_snapshotTurn}");
            }
        }

        public async void SkipMove()
        {
            try
            {
                var txHash = await _dojoGameManager.GameSystem.skip_move(_localPlayerAccount);
                CustomLogger.LogEvent($"[Skip Move]: Hash {txHash} Account {_localPlayerAccount.Address.Hex()} skipped a move");
                
            } catch (Exception e)
            {
                CustomLogger.LogError($"Error skipping move: {e.Message}");
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