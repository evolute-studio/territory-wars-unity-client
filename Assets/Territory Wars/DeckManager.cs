using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TerritoryWars
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
                { "CCRF", 4 },  
                { "CCFR", 4 },  
                { "CFRF", 11 }, 
                { "CRRF", 9 },  
                { "CRFF", 9 },  
                { "FFCR", 4 },  
                { "FFRF", 4 },  
                { "FRRF", 9 },  
                { "FFCC", 4 },  
                { "RRFF", 6 }   
            };
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

            // Перемішуємо список
            for (int i = allTiles.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (allTiles[i], allTiles[randomIndex]) = (allTiles[randomIndex], allTiles[i]);
            }

            // Створюємо чергу з перемішаних тайлів
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

        // Метод для отримання інформації про залишок тайлів кожного типу
        public Dictionary<string, int> GetRemainingTileTypes()
        {
            return tileDeck.GroupBy(t => t.id)
                          .ToDictionary(g => g.Key, g => g.Count());
        }

        // Метод для перемішування залишку колоди
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