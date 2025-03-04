using System;
using System.Threading.Tasks;
using Dojo;
using Dojo.Starknet;
using Dojo.Torii;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.Dojo
{
    public class CustomSynchronizationMaster : MonoBehaviour
    {
        public WorldManager WorldManager;

        #region Sync Methods

        // -------- Sync Methods --------
        public async Task<int> SyncPlayer(FieldElement address)
        {
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
            int count = await SyncConstruction(DojoQueries.GetQueryOnlyBoardWithDependencies(board_id), nameof(SyncBoardWithDependencies));
            CustomLogger.LogDojoLoop($"Synced {count} boards and dependencies with board id {board_id}");
            return count;
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
        
        #endregion
        
        public void DestroyPlayersExceptLocal(FieldElement localAddress)
        {
            GameObject[] players = WorldManager.Entities<evolute_duel_Player>();
            foreach (GameObject player in players)
            {
                evolute_duel_Player playerModel = player.GetComponent<evolute_duel_Player>();
                if (playerModel.player_id != localAddress)
                {
                    Destroy(player);
                    WorldManager.RemoveEntity(player.name);
                }
            }
        }
        
        public void DestroyBoardsAndAllDependencies()
        {
            GameObject[] boards = WorldManager.Entities<evolute_duel_Board>();
            foreach (GameObject board in boards)
            {
                evolute_duel_Board boardModel = board.GetComponent<evolute_duel_Board>();
                Destroy(board);
                WorldManager.RemoveEntity(board.name);
            }
            GameObject[] moves = WorldManager.Entities<evolute_duel_Move>();
            foreach (GameObject move in moves)
            {
                Destroy(move);
                WorldManager.RemoveEntity(move.name);
            }
            GameObject[] cityNodes = WorldManager.Entities<evolute_duel_CityNode>();
            foreach (GameObject cityNode in cityNodes)
            {
                Destroy(cityNode);
                WorldManager.RemoveEntity(cityNode.name);
            }
            GameObject[] roadNodes = WorldManager.Entities<evolute_duel_RoadNode>();
            foreach (GameObject roadNode in roadNodes)
            {
                Destroy(roadNode);
                WorldManager.RemoveEntity(roadNode.name);
            }
        }

        public void DestroyAllGames()
        {
            GameObject[] games = WorldManager.Entities<evolute_duel_Game>();
            foreach (GameObject game in games)
            {
                Destroy(game);
                WorldManager.RemoveEntity(game.name);
            }
        }
        
        // -------- Helper Methods --------

        public async Task<int> SyncConstruction(Query query, string type)
        {
            try
            {
                int result = await WorldManager.synchronizationMaster.SynchronizeEntities(query);
                return result;
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
    }
}