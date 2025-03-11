using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dojo;
using Dojo.Starknet;
using Dojo.Torii;
using TerritoryWars.Tools;
using UnityEngine;
using System.Collections;
using TerritoryWars.General;

namespace TerritoryWars.Dojo
{
    public class CustomSynchronizationMaster : MonoBehaviour
    {
        public WorldManager WorldManager;

        #region Sync Methods

        // -------- Sync Methods --------
        public async Task<int> SyncPlayer(FieldElement address)
        {
            CustomLogger.LogDojoLoop($"Syncing player with address {address}");
            int count = await SyncConstruction(
                DojoQueries.GetQueryPlayer(address), 
                nameof(SyncPlayer));
            CustomLogger.LogDojoLoop($"Synced {count} players with address {address}");
            return count;
        }
        
        public async Task<int> SyncPlayersArray(FieldElement[] address)
        {
            int count = await SyncConstruction(
                DojoQueries.GetQueryPlayersArray(address), 
                nameof(SyncPlayer));
            CustomLogger.LogDojoLoop($"Synced {count} players in array with addresses count {address.Length}");
            return count;
        }
        
        public async Task<int> SyncPlayerInProgressGame(FieldElement address)
        {
            int count = await SyncConstruction(
                DojoQueries.GetQueryPlayerInProgressGame(address), 
                nameof(SyncPlayerInProgressGame));
            CustomLogger.LogDojoLoop($"Synced {count} InProgress games with player address {address}");
            return count;
        }
        
        public async Task<int> SyncGeneralModels()
        {
            int count = await SyncConstruction(DojoQueries.GetGeneralModels(), nameof(SyncGeneralModels));
            CustomLogger.LogDojoLoop($"Synced {count} general models");
            return count;
        }
        
        public async Task<int> SyncBoardWithDependencies(FieldElement board_id)
        {
            int count = await SyncConstruction(DojoQueries.GetQueryBoardWithDependencies(board_id), nameof(SyncBoardWithDependencies));
            CustomLogger.LogDojoLoop($"Synced {count} boards and dependencies with board id {board_id}");
            return count;
        }
        
        public async Task<int> SyncMoveByFirstBoardId(FieldElement board_id)
        {
            int count = await SyncConstruction(DojoQueries.GetQueryMoveByFirstBoardId(board_id), nameof(SyncMoveByFirstBoardId));
            CustomLogger.LogDojoLoop($"Synced {count} moves with board id {board_id}");
            return count;
        }
        
        public async Task<int> SyncMoveById(FieldElement id)
        {
            int count = await SyncConstruction(DojoQueries.GetQueryMoveById(id), nameof(SyncMoveById));
            CustomLogger.LogDojoLoop($"Synced {count} moves with move id {id}");
            return count;
        }
        
        public async Task<int> SyncMovesByFirstBoardId(FieldElement board_id)
        {
            int count = await SyncConstruction(DojoQueries.GetQueryMoveByFirstBoardId(board_id), nameof(SyncMovesByFirstBoardId));
            CustomLogger.LogDojoLoop($"Synced {count} moves with board id {board_id}");
            return count;
        }
        
        public async Task<int> SyncMovesArray(FieldElement[] move_ids)
        {
            int count = await SyncConstruction(DojoQueries.GetQueryMoveByIdArray(move_ids), nameof(SyncMovesArray));
            CustomLogger.LogDojoLoop($"Synced {count} moves in array with move ids count {move_ids.Length}");
            return count;
        }

        public async Task<HashSet<string>> SyncAllMoveByBoardId(FieldElement board_id)
        {
            if (board_id == null)
            {
                CustomLogger.LogError("Board id is null");
                return null;
            }
            HashSet<evolute_duel_Move> moves = new HashSet<evolute_duel_Move>();
            HashSet<string> processedBoardIds = new HashSet<string>();
            
            var boardsToProcess = new Queue<FieldElement>();
            boardsToProcess.Enqueue(board_id);
            
            evolute_duel_Board board = DojoGameManager.Instance.WorldManager.Entities<evolute_duel_Board>()
                .FirstOrDefault(b => b.GetComponent<evolute_duel_Board>().id == board_id)?
                .GetComponent<evolute_duel_Board>();
            if (board == null)
            {
                CustomLogger.LogError($"Sync moves. Board with id {board_id} not found");
                return null;
            }

            FieldElement lastMoveId = board.last_move_id switch
            {
                Option<FieldElement>.Some some => some.value,
                Option<FieldElement>.None => null,
                _ => null
            };
            if (lastMoveId == null)
            {
                CustomLogger.LogWarning("Board has no last move id. Snapshot session hasn't any moves");
                return null;
            }
            await SyncMoveById(lastMoveId);

            while (boardsToProcess.Count > 0 && processedBoardIds.Count < 64)
            {
                var currentBoardId = boardsToProcess.Dequeue();
                if (processedBoardIds.Contains(currentBoardId.Hex()))
                    continue;
                IncomingModelsFilter.AddBoardToAllowedBoards(currentBoardId.Hex());
                processedBoardIds.Add(currentBoardId.Hex());
                
                CustomLogger.LogDojoLoop($"Processing board id {currentBoardId.Hex()}");
                
                int count = await SyncMovesByFirstBoardId(currentBoardId);
                CustomLogger.LogDojoLoop($"Synced {count} moves with board id {currentBoardId.Hex()}");
                
                var moveObjects = WorldManager.Entities<evolute_duel_Move>();
                List<FieldElement> prevMoveIds = new List<FieldElement>();
                foreach (var moveObject in moveObjects)
                {
                    if (!moveObject.TryGetComponent<evolute_duel_Move>(out var move))
                        continue;

                    FieldElement prevMoveId = move.prev_move_id switch
                    {
                        Option<FieldElement>.Some some => some.value,
                        Option<FieldElement>.None => null,
                        _ => null
                    };
                    if (prevMoveId == null)
                        continue;
                    prevMoveIds.Add(prevMoveId);
                }
                count = await SyncMovesArray(prevMoveIds.ToArray());
                moveObjects = WorldManager.Entities<evolute_duel_Move>();
                
                foreach (var moveObject in moveObjects)
                {
                    if (!moveObject.TryGetComponent<evolute_duel_Move>(out var move))
                        continue;

                    if (!moves.Contains(move))
                    {
                        moves.Add(move);
                        
                        if (!processedBoardIds.Contains(move.first_board_id.Hex()))
                        {
                            boardsToProcess.Enqueue(move.first_board_id);
                        }
                    }
                }
                
                CustomLogger.LogDojoLoop($"Synced {count} moves with board id {currentBoardId.Hex()}. " +
                                       $"Total moves: {moves.Count}, Processed boards: {processedBoardIds.Count}");
            }

            if (processedBoardIds.Count >= 64)
            {
                CustomLogger.LogWarning("Max board limit reached (64). Some moves might be missing.");
            }

            return processedBoardIds;
        }
        
        public async Task<int> SyncOnlyBoard(FieldElement board_id)
        {
            int count = await SyncConstruction(DojoQueries.GetQueryOnlyBoard(board_id), nameof(SyncOnlyBoard));
            CustomLogger.LogDojoLoop($"Synced {count} board with board id {board_id}");
            return count;
        }
        
        public async Task<int> SyncCreatedGame()
        {
            int count = await SyncConstruction(DojoQueries.GetQueryCreatedGame(), nameof(SyncCreatedGame));
            CustomLogger.LogDojoLoop($"Synced {count} created games");
            return count;
        }
        
        public async Task<int> SyncPlayerSnapshots(FieldElement address)
        {
            int count = await SyncConstruction(DojoQueries.GetQueryPlayerSnapshots(address), nameof(SyncPlayerSnapshots));
            CustomLogger.LogDojoLoop($"Synced {count} player snapshots with address {address}");
            return count;
        }
        
        public async Task<int> SyncSnapshotArray(FieldElement[] snapshot_ids)
        {
            int count = await SyncConstruction(DojoQueries.GetQuerySnapshotArray(snapshot_ids), nameof(SyncSnapshotArray));
            CustomLogger.LogDojoLoop($"Synced {count} snapshots in array with snapshot ids count {snapshot_ids.Length}");
            return count;
        }
        
        #endregion
        
        public void DestroyPlayersExceptLocal(FieldElement localAddress)
        {
            GameObject[] players = WorldManager.Entities<evolute_duel_Player>();
            foreach (GameObject player in players)
            {
                evolute_duel_Player playerModel = player.GetComponent<evolute_duel_Player>();
                if (playerModel.player_id.Hex() != localAddress.Hex())
                {
                    DestroyImmediate(player);
                    //WorldManager.RemoveEntity(player.name);
                }
            }
        }
        
        public void DestroyBoardsAndAllDependencies()
        {
            GameObject[] boards = WorldManager.Entities<evolute_duel_Board>();
            foreach (GameObject board in boards)
            {
                evolute_duel_Board boardModel = board.GetComponent<evolute_duel_Board>();
                DestroyImmediate(board);
                //WorldManager.RemoveEntity(board.name);
            }
            GameObject[] moves = WorldManager.Entities<evolute_duel_Move>();
            foreach (GameObject move in moves)
            {
                DestroyImmediate(move);
                //WorldManager.RemoveEntity(move.name);
            }
            GameObject[] cityNodes = WorldManager.Entities<evolute_duel_CityNode>();
            foreach (GameObject cityNode in cityNodes)
            {
                DestroyImmediate(cityNode);
                //WorldManager.RemoveEntity(cityNode.name);
            }
            GameObject[] roadNodes = WorldManager.Entities<evolute_duel_RoadNode>();
            foreach (GameObject roadNode in roadNodes)
            {
                DestroyImmediate(roadNode);
                //WorldManager.RemoveEntity(roadNode.name);
            }
        }

        public void DestroyAllGames()
        {
            GameObject[] games = WorldManager.Entities<evolute_duel_Game>();
            foreach (GameObject game in games)
            {
                evolute_duel_Game gameModel = game.GetComponent<evolute_duel_Game>();
                gameModel.OnUpdated.RemoveAllListeners();
                DestroyImmediate(game);
                //WorldManager.RemoveEntity(game.name);
            }
        }
        
        public void DestroyAllSnapshots()
        {
            evolute_duel_Snapshot[] snapshots = WorldManager.Entities<evolute_duel_Snapshot>().Select(s => s.GetComponent<evolute_duel_Snapshot>()).ToArray();
            foreach (evolute_duel_Snapshot snapshot in snapshots)
            {
                snapshot.OnUpdated.RemoveAllListeners();
                DestroyImmediate(snapshot.gameObject);
                //WorldManager.RemoveEntity(snapshot.name);
            }
        }
        
        // -------- Helper Methods --------

        public async Task<int> SyncConstruction(Query query, string type)
        {
            try
            {
                var syncTask = WorldManager.synchronizationMaster.SynchronizeEntities(query);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
                
                var completedTask = await Task.WhenAny(syncTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    CustomLogger.LogError($"Sync {type} operation timed out after 30 seconds");
                    return -2; // спеціальний код помилки для таймауту
                }
                
                return await syncTask;
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Failed to sync {type} with error: {e.Message}");
                return -1;
            }
        }

        public void DestroyModels<T>() where T : ModelInstance
        {
            GameObject[] players = WorldManager.Entities<T>();
            foreach (GameObject player in players)
            {
                Destroy(player);
            }
        }

        public async Task<T> WaitForModel<T>(string entityId, float timeoutSeconds = 10f) where T : ModelInstance
        {
            var tcs = new TaskCompletionSource<T>();
            StartCoroutine(WaitForModelCoroutine<T>(
                entityId,
                model => tcs.TrySetResult(model),
                () => tcs.TrySetResult(null),
                timeoutSeconds
            ));
            return await tcs.Task;
        }

        private IEnumerator WaitForModelCoroutine<T>(
            string entityId, 
            Action<T> onFound,
            Action onTimeout,
            float timeoutSeconds) where T : ModelInstance
        {
            float startTime = Time.time;
            while (Time.time - startTime < timeoutSeconds)
            {
                var entity = WorldManager.Entities<T>().FirstOrDefault(e => e.name == entityId);
                if (entity != null && entity.TryGetComponent<T>(out var model))
                {
                    onFound(model);
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            CustomLogger.LogError($"Timeout waiting for model {typeof(T).Name} with id {entityId}");
            onTimeout();
        }

        public async Task<T> WaitForModelByPredicate<T>(Func<T, bool> predicate, float timeoutSeconds = 10f) where T : ModelInstance
        {
            var tcs = new TaskCompletionSource<T>();
            StartCoroutine(WaitForModelByPredicateCoroutine<T>(
                predicate,
                model => tcs.TrySetResult(model),
                () => tcs.TrySetResult(null),
                timeoutSeconds
            ));
            return await tcs.Task;
        }

        private IEnumerator WaitForModelByPredicateCoroutine<T>(
            Func<T, bool> predicate,
            Action<T> onFound,
            Action onTimeout,
            float timeoutSeconds) where T : ModelInstance
        {
            float startTime = Time.time;
            while (Time.time - startTime < timeoutSeconds)
            {
                var entities = WorldManager.Entities<T>();
                foreach (var entity in entities)
                {
                    if (entity.TryGetComponent<T>(out var model) && predicate(model))
                    {
                        onFound(model);
                        yield break;
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            CustomLogger.LogError($"Timeout waiting for model {typeof(T).Name} matching predicate");
            onTimeout();
        }
    }
}