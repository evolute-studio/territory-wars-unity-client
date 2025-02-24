using System;
using System.Collections.Generic;
using System.Linq;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.Carts
{
    public class CartsSystem : MonoBehaviour
    {
        public Board board;

        public static float CartsSpeed = 0.1f;
        public GameObject PlayerCartPrefab;
        
        public Sprite FirstPlayerCartSprite;
        public Sprite SecondPlayerCartSprite;

        public static Sprite FirstPlayerCartSpriteStatic;
        public static Sprite SecondPlayerCartSpriteStatic;

        public List<CartAnimator> FirstPlayerCartSprites;
        public List<CartAnimator> SecondPlayerCartSprites;
        
        public static List<CartAnimator> FirstPlayerCartSpritesStatic;
        public static List<CartAnimator> SecondPlayerCartSpritesStatic;



        // Змінюємо на Dictionary для швидкого доступу по координатах
        private Dictionary<Vector2Int, RoadTile> roadTiles = new Dictionary<Vector2Int, RoadTile>();

        public void Start() => Initialize();

        public void Initialize()
        {
            FirstPlayerCartSpriteStatic = FirstPlayerCartSprite;
            SecondPlayerCartSpriteStatic = SecondPlayerCartSprite;
            FirstPlayerCartSpritesStatic = FirstPlayerCartSprites;
            SecondPlayerCartSpritesStatic = SecondPlayerCartSprites;
            board.OnTilePlaced += OnTilePlaced;
        }
        
        [Serializable]
        public class CartAnimator
        {
            public List<Sprite> CartSprites;
            public Vector3 Direction;
            public bool IsMirrored;
        }

        public void OnTilePlaced(TileData tileData, int x, int y)
        {
            if (!tileData.id.Contains('R'))
            {
                return;
            }

            GameObject tileObject = board.GetTileObject(x, y);
            Transform cartsPath = tileObject.GetComponent<TileGenerator>().RoadPath.transform;
            int playerId = General.SessionManager.Instance.CurrentTurnPlayer.LocalId;
            RoadTile roadTile = new RoadTile(playerId, tileObject, tileData, cartsPath);
            
            //int cartsCount = tileData.id.Count(c => c == 'R');
            int cartsCount = 1;
            Cart[] carts = new Cart[cartsCount];
            for (int i = 0; i < cartsCount; i++)
            {
                GameObject cartObject = Instantiate(PlayerCartPrefab, tileObject.transform);
                cartObject.SetActive(false);
                carts[i] = new Cart(cartObject);
            }
            roadTile.AddCart(carts);

            // Додаємо тайл до словника і оновлюємо зв'язки
            Vector2Int position = new Vector2Int(x, y);
            roadTiles[position] = roadTile;
            UpdateTileConnections(roadTile, position);
        }

        private void UpdateTileConnections(RoadTile newTile, Vector2Int position)
        {
            // Отримуємо першу і останню точку шляху
            Vector3 pathStart = newTile.path[0];
            Vector3 pathEnd = newTile.path[newTile.path.Length - 1];

            // Перевіряємо всі сусідні клітинки
            Vector2Int[] neighbors = new[]
            {
                position + Vector2Int.up,
                position + Vector2Int.right,
                position + Vector2Int.down,
                position + Vector2Int.left
            };

            foreach (Vector2Int neighborPos in neighbors)
            {
                if (!roadTiles.TryGetValue(neighborPos, out RoadTile neighborTile))
                    continue;

                Vector3 neighborStart = neighborTile.path[0];
                Vector3 neighborEnd = neighborTile.path[neighborTile.path.Length - 1];

                // Перевіряємо з'єднання кінця нового тайлу з початком сусіднього
                if (Vector3.Distance(pathEnd, neighborStart) < 0.1f)
                {
                    newTile.NextTile = neighborTile;
                    neighborTile.PreviousTile = newTile;
                }
                // Перевіряємо з'єднання початку нового тайлу з кінцем сусіднього
                else if (Vector3.Distance(pathStart, neighborEnd) < 0.1f)
                {
                    newTile.PreviousTile = neighborTile;
                    neighborTile.NextTile = newTile;
                }
            }
        }

        public void Update()
        {
            foreach (var roadTile in roadTiles.Values)
            {
                roadTile.TrySpawnCarts();
                roadTile.MoveCarts();
                roadTile.RefreshCartDirection();
            }
        }

        // Офни якщо тобі не треба відображати інформацію про карт
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
        
            foreach (var roadTile in roadTiles.Values)
            {
                roadTile.DrawCartsInfo();
            }
        }
    }

    
    
}
