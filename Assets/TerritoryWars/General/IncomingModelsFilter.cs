using System.Collections.Generic;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.Dojo;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace TerritoryWars.General
{
    public static class IncomingModelsFilter
    {
        public static string LocalPlayerId;
        public static string CurrentBoardId;
        public static List<string> AllowedBoards = new List<string>();
        public static List<string> AllowedPlayers = new List<string>();
        
        public static UnityEvent<ModelInstance> OnModelPassed = new UnityEvent<ModelInstance>();
        
        public static void SetLocalPlayerId(string localPlayerId)
        {
            LocalPlayerId = localPlayerId;
        }
        
        public static void SetSessionCurrentBoardId(string currentBoardId)
        {
            CurrentBoardId = currentBoardId;
            
            if(!AllowedPlayers.Contains(LocalPlayerId))
                AllowedPlayers.Add(LocalPlayerId);
        }
        
        public static void SetSessionAllowedBoards(List<string> allowedBoards)
        {
            AllowedBoards = allowedBoards;
            if(!AllowedPlayers.Contains(LocalPlayerId))
                AllowedPlayers.Add(LocalPlayerId);
        }
        
        public static void AddBoardToAllowedBoards(string boardId)
        {
            if(!AllowedBoards.Contains(boardId))
                AllowedBoards.Add(boardId);
        }
        
        public static void SetSessionPlayers(List<string> currentPlayersInSession)
        {
            AllowedPlayers = currentPlayersInSession;
        }
        

        public static void FilterModels(ModelInstance model)
        {
            switch (ApplicationState.CurrentState)
            {
                case ApplicationStates.Menu:
                    if (!MenuFilter(model))
                    {
                        DestroyModel(model);
                        return;
                    }
                    break;
                case ApplicationStates.MatchTab:
                    if (!MatchTabFilter(model))
                    {
                        DestroyModel(model);
                        return;
                    }
                    break;
                case ApplicationStates.SnapshotTab:
                    if (!MenuFilter(model))
                    {
                        DestroyModel(model);
                        return;
                    }
                    break;
                case ApplicationStates.Session:
                    if (!SessionFilter(model))
                    {
                        DestroyModel(model);
                        return;
                    }
                    break;
            }
            OnModelPassed.Invoke(model);
        }
        
        private static void DestroyModel(ModelInstance model)
        {
            CustomLogger.LogInfo($"Filter Mode: [{ApplicationState.CurrentState}] Destroying model. Model type: " + model.GetType().Name);
            GameObject gameObject = model.gameObject;
            Component[] components = model.gameObject.GetComponents<Component>();
            string s = $"GameObject: {model.gameObject.name} Before: Components in game object: ";
            foreach (var component in components)
            {
                s += component.GetType().Name + ", ";
            }
            
            s += " | ";
            
            
            model.OnUpdated.RemoveAllListeners();
            //DojoGameManager.Instance.WorldManager.RemoveEntity(model.gameObject.name);
            Object.DestroyImmediate(model);
            CustomLogger.LogInfo("Model destroyed");
            
            s += "After: Components in game object: ";
            foreach (var component in components)
            {
                s += component.GetType().Name + ", ";
            }
            
            CustomLogger.LogInfo(s);
            if(components.Length == 2)
            {
                CustomLogger.LogInfo("Destroying game object");
                Object.DestroyImmediate(gameObject);
            }
        }
        
        private static void LogComponents(string stage, GameObject gameObject)
        {
            if (gameObject == null) return;
            
            var components = gameObject.GetComponents<Component>();
            var componentNames = components
                .Where(c => c != null)
                .Select(c => c.GetType().Name)
                .ToList();
            
            CustomLogger.LogInfo($"{stage}: GameObject {gameObject.name} has components: {string.Join(", ", componentNames)}");
        }
        
        public static bool MenuFilter(ModelInstance model)
        {
            var type = model.GetType();
            switch (type.Name)
            {
                // Have condition
                case nameof(evolute_duel_Game): 
                    evolute_duel_Game game = (evolute_duel_Game)model;
                    bool isCreated = game.status switch
                    {
                        GameStatus.Created => true,
                        _ => false
                    };
                    return isCreated;
                case nameof(evolute_duel_Snapshot): 
                    evolute_duel_Snapshot snapshot = (evolute_duel_Snapshot)model;
                    if(snapshot.player.Hex() == LocalPlayerId)
                        return true;
                    return false;
                case nameof(evolute_duel_Player):
                    evolute_duel_Player player = (evolute_duel_Player)model;
                    if(player.player_id.Hex() == LocalPlayerId)
                        return true;
                    return false;
                
                // Always allow
                case nameof(evolute_duel_Rules):
                    return true;
                case nameof(evolute_duel_Shop):
                    return true;
                
                // Always deny
                case nameof(evolute_duel_Board):
                    return false;
                case nameof(evolute_duel_Move):
                    return false;
                case nameof(evolute_duel_PotentialCityContests):
                    return false;
                case nameof(evolute_duel_CityNode):
                    return false;
                case nameof(evolute_duel_PotentialRoadContests):
                    return false;
                case nameof(evolute_duel_RoadNode):
                    return false;
                
                default:
                    CustomLogger.LogWarning($"Unknown model type {type.Name}. Filtering out.");
                    return true;
            }
        }
        
        public static bool MatchTabFilter(ModelInstance model)
        {
            var type = model.GetType();
            switch (type.Name)
            {
                // Have condition
                case nameof(evolute_duel_Game): 
                    evolute_duel_Game game = (evolute_duel_Game)model;
                    bool isCreated = game.status switch
                    {
                        GameStatus.Created => true,
                        GameStatus.Canceled => true,
                        _ => false
                    };
                    return isCreated;
                case nameof(evolute_duel_Snapshot): 
                    evolute_duel_Snapshot snapshot = (evolute_duel_Snapshot)model;
                    if(snapshot.player.Hex() == LocalPlayerId)
                        return true;
                    return false;
                
                // Always allow
                case nameof(evolute_duel_Rules):
                    return true;
                case nameof(evolute_duel_Shop):
                    return true;
                case nameof(evolute_duel_Player):
                    return true;
                
                // Always deny
                case nameof(evolute_duel_Board):
                    return false;
                case nameof(evolute_duel_Move):
                    return false;
                case nameof(evolute_duel_PotentialCityContests):
                    return false;
                case nameof(evolute_duel_CityNode):
                    return false;
                case nameof(evolute_duel_PotentialRoadContests):
                    return false;
                case nameof(evolute_duel_RoadNode):
                    return false;
                
                
                default:
                    CustomLogger.LogWarning($"Unknown model type {type.Name}. Filtering out.");
                    return true;
            }
        }

        public static bool SessionFilter(ModelInstance model)
        {
            var type = model.GetType();
            switch (type.Name)
            {
                // Have condition
                case nameof(evolute_duel_Board):
                    evolute_duel_Board board = (evolute_duel_Board)model;
                    if(board.id == null)
                    {
                        if (GameObject.Find(model.gameObject.name) != null)
                        {
                            CustomLogger.LogWarning("Board id is null, But client has the board. Allowing.");
                            return true;
                        }
                        else
                        {
                            CustomLogger.LogWarning("Board id is null, And client doesn't have the board. Denying.");
                            return false;
                        }
                        
                    }
                    CustomLogger.LogInfo($"Checking board {board.id.Hex()}");
                    CustomLogger.LogInfo($"Allowed boards: {AllowedBoards.Count}");
                    if(AllowedBoards.Contains(board.id.Hex()))
                        return true;
                    return false;
                case nameof(evolute_duel_Move):
                    evolute_duel_Move move = (evolute_duel_Move)model;
                    CustomLogger.LogInfo($"Checking move board first id {move.first_board_id.Hex()}");
                    bool isAllowed = AllowedBoards.Contains(move.first_board_id.Hex());
                    CustomLogger.LogInfo($"Is move allowed: {isAllowed}");
                    if(AllowedBoards.Contains(move.first_board_id.Hex()))
                        return true;
                    return false;
                case nameof(evolute_duel_CityNode):
                    evolute_duel_CityNode cityNode = (evolute_duel_CityNode)model;
                    if(AllowedBoards.Contains(cityNode.board_id.Hex()))
                        return true;
                    return false;
                case nameof(evolute_duel_RoadNode):
                    evolute_duel_RoadNode roadNode = (evolute_duel_RoadNode)model;
                    if(AllowedBoards.Contains(roadNode.board_id.Hex()))
                        return true;
                    return false;
                case nameof(evolute_duel_Player):
                    evolute_duel_Player player = (evolute_duel_Player)model;
                    if(AllowedPlayers.Contains(player.player_id.Hex()))
                        return true;
                    return false;
                case nameof(evolute_duel_Game): 
                    evolute_duel_Game game = (evolute_duel_Game)model;
                    if(game.player.Hex() == LocalPlayerId)
                        return true;
                    return false;
                
                // Always allow
                case nameof(evolute_duel_Rules):
                    return true;
                case nameof(evolute_duel_Shop):
                    return true;
                
                // Always deny
                case nameof(evolute_duel_Snapshot):
                    return false;
                case nameof(evolute_duel_PotentialCityContests):
                    return false;
                case nameof(evolute_duel_PotentialRoadContests):
                    return false;
                
                default:
                    CustomLogger.LogWarning($"Unknown model type {type.Name}. Filtering out.");
                    return true;
            }
        }
    }
}