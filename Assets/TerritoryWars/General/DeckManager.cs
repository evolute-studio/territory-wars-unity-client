using System.Collections.Generic;
using System.Linq;
using TerritoryWars.Tile;
using UnityEngine;

namespace TerritoryWars.General
{
    public class DeckManager : MonoBehaviour
    {
        private Queue<TileData> tileDeck;
        private Dictionary<string, int> tileConfig;

        private void Awake()
        {
            InitializeTileConfig();
            ShuffleDeck();
        }

        private void InitializeTileConfig()
        {
            tileConfig = new Dictionary<string, int>
            {
                { "CCCC", 1 },
                { "CCFC", 4 },
                { "CCRC", 3 },
                { "CFFC", 7 },
                { "CRRC", 6 },
                { "CFCF", 6 },
                { "CFFF", 4 },
                { "CFRR", 4 },
                { "CRRF", 3 },
                { "CRFR", 4 },
                { "RFRF", 8 },
                { "FFRR", 9 },
                { "FRRR", 4 },
                { "FFFF", 1 }
            };
            // tileConfig = new Dictionary<string, int>
            // {
            //     { "RRFF", 8},
            //     { "RRRF", 8},
            //     { "RFRF", 8},
            //     
            // };
        }

        private void ShuffleDeck()
        {
            List<TileData> allTiles = new List<TileData>();

            foreach (var tileType in tileConfig)
            {
                for (int i = 0; i < tileType.Value; i++)
                {
                    allTiles.Add(new TileData(tileType.Key));
                }
            }

            
            for (int i = allTiles.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (allTiles[i], allTiles[randomIndex]) = (allTiles[randomIndex], allTiles[i]);
            }

            
            tileDeck = new Queue<TileData>(allTiles);
        }

        public TileData DrawTile()
        {
            if (tileDeck.Count == 0)
            {
                Debug.Log("Колода порожня!");
                return null;
            }

            return tileDeck.Dequeue();
        }

        public int RemainingTiles => tileDeck.Count;

        public bool HasTiles => tileDeck.Count > 0;

        
        public Dictionary<string, int> GetRemainingTileTypes()
        {
            return tileDeck.GroupBy(t => t.id)
                          .ToDictionary(g => g.Key, g => g.Count());
        }

        
        public void ReshuffleDeck()
        {
            var remainingTiles = tileDeck.ToList();
            tileDeck.Clear();

            for (int i = remainingTiles.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (remainingTiles[i], remainingTiles[randomIndex]) =
                    (remainingTiles[randomIndex], remainingTiles[i]);
            }

            tileDeck = new Queue<TileData>(remainingTiles);
        }
    }
}