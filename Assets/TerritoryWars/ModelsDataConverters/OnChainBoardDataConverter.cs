using System;
using System.Collections.Generic;
using System.Linq;
using TerritoryWars.Tools;
using UnityEngine;
using Random = System.Random;

namespace TerritoryWars.ModelsDataConverters
{
    public static class OnChainBoardDataConverter
    {
        public static char[] EdgeTypes = { 'C', 'R', 'M', 'F' };

        public static string[] TileTypes =
        {
            "CCCC", "FFFF", "RRRR", "CCCF", "CCCR", "CCRR", "CFFF", "FFFR", "CRRR", "FRRR",
            "CCFF", "CFCF", "CRCR", "FFRR", "FRFR", "CCFR", "CCRF", "CFCR", "CFFR", "CFRF",
            "CRFF", "CRRF", "CRFR", "CFRR"
        };
        
        public static string[] InDeckTileTypes =
        {
            "CCCC",
            "CCCF",
            "CCCR",
            "CCFF",
            "CCRR",
            "CFCF",
            "CFFF",
            "CFRR",
            "CRRF",
            "CRFR",
            "FRFR",
            "FFRR",
            "FRRR",
            "FFFF"
        };
    
        public static char[] GetInitialEdgeState(byte[] initial_edge_state)
        {
            char[] charArray = Array.ConvertAll(initial_edge_state, c => EdgeTypes[(int)c]);
            return charArray;
        }
    
        public static string GetTopTile(Option<byte> top_tile)
        {
            if (top_tile is Option<byte>.Some some)
            {
                return TileTypes[some.value];
            }
            return "None";
        }
        
        public static (byte, byte) GetTypeAndRotation(string config)
        {
            for (byte rotation = 0; rotation < 4; rotation++)
            {
                string rotatedConfig = Rotate(config, rotation);
                int index = Array.IndexOf(TileTypes, rotatedConfig);
                if (index != -1)
                {
                    return ((byte)index, rotation);
                }
            }

            CustomLogger.LogError("Tile type not found: " + config);
            return (0, 0);
        }

        private static string Rotate(string config, int times)
        {
            if (string.IsNullOrEmpty(config) || times < 0 || times >= config.Length)
                return config;
        
            return config.Substring(times) + config.Substring(0, times);
        }
        
        public static Random Random = new Random();
        
        public static (string, int) GetRandomTypeAndRotationFromDeck(string config)
        {
            if (string.IsNullOrEmpty(config))
            {
                CustomLogger.LogWarning("Config pattern is empty");
                return (null, 0);
            }

            var matchingConfigs = new List<(string tile, int rotation)>();
            
            // Перебираємо всі тайли з колоди
            foreach (var deckTile in InDeckTileTypes)
            {
                // Для кожного тайлу перевіряємо всі можливі ротації
                for (int rotationI = 0; rotationI < 4; rotationI++)
                {
                    string rotatedTile = Rotate(deckTile, rotationI);
                    bool isMatch = true;
                    
                    // Перевіряємо кожен символ в конфігурації
                    for (int i = 0; i < 4; i++)
                    {
                        if (config[i] != 'X' && config[i] != rotatedTile[i])
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    
                    if (isMatch)
                    {
                        // Зберігаємо оригінальний тайл та кількість поворотів
                        matchingConfigs.Add((deckTile, rotationI));
                        Debug.Log($"Found matching config: {deckTile} with rotation {rotationI}. Original: {config}, Rotated: {rotatedTile}");
                    }
                }
            }

            if (matchingConfigs.Count == 0)
            {
                CustomLogger.LogWarning($"No matching configurations found for pattern: {config}");
                return (null, 0);
            }

            // Вибираємо випадкову конфігурацію
            var randomIndex = Random.Next(matchingConfigs.Count);
            var (selectedTile, rotation) = matchingConfigs[randomIndex];
            
            Debug.Log($"Selected tile: {selectedTile} with rotation {rotation} for pattern: {config}");
            
            return (selectedTile, rotation);
        }

        private static bool MatchesPattern(string tile, string pattern)
        {
            for (int i = 0; i < 4; i++)
            {
                if (pattern[i] != 'X' && pattern[i] != tile[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static IEnumerable<string> GetAllRotations(string tile)
        {
            for (int i = 0; i < 4; i++)
            {
                yield return Rotate(tile, i);
            }
        }

        private static int GetRotation(string[] tileDeck, string tile)
        {
            for (int i = 0; i < tileDeck.Length; i++)
            {
                for (int r = 0; r < 4; r++)
                {
                    if (Rotate(tileDeck[i], r) == tile)
                    {
                        return r;
                    }
                }
            }
            return 0;
        }

        public static Vector2Int GetPositionByRoot(byte root)
        {
            int tile = root / 4;
            int x = tile / 8;
            int y = tile % 8;
            return new Vector2Int(x + 1, y + 1);
        }
    }
}