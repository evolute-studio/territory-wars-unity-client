using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TerritoryWars
{
    public class ValidPlacement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Rotation { get; set; }
    }

    public class Board : MonoBehaviour
    {
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private GameObject tilePrefab;

        private GameObject[,] tileObjects;
        private TileData[,] tileData;

        private void Awake()
        {
            InitializeBoard();
            CreateRandomBorder();
        }

        private void InitializeBoard()
        {
            tileObjects = new GameObject[width, height];
            tileData = new TileData[width, height];

            // Створюємо сітку тайлів
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float xPosition = (x - y) * 0.5f;
                    float yPosition = (x + y) * 0.25f;
                    Vector3 position = new Vector3(xPosition, yPosition, 0);

                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                    tile.name = $"Tile_{x}_{y}";
                    tileObjects[x, y] = tile;
                    tile.SetActive(false);
                }
            }
        }

        private void CreateRandomBorder()
        {
            // Список всіх можливих тайлів
            string[] possibleTiles = new string[]
            {
                "CFFR", "CFRF", "CRFF", "CFFF",
                "RFFR", "RFRF", "RRFF", "RFFF",
                "FFFF" // Просте поле
            };

            // Проходимо по контуру за годинниковою стрілкою
            // Верхня сторона (зліва направо)
            for (int x = 0; x < width; x++)
            {
                TryPlaceRandomTile(possibleTiles, x, 0);
            }

            // Права сторона (зверху вниз)
            for (int y = 1; y < height; y++)
            {
                TryPlaceRandomTile(possibleTiles, width - 1, y);
            }

            // Нижня сторона (справа наліво)
            for (int x = width - 2; x >= 0; x--)
            {
                TryPlaceRandomTile(possibleTiles, x, height - 1);
            }

            // Ліва сторона (знизу вгору)
            for (int y = height - 2; y > 0; y--)
            {
                TryPlaceRandomTile(possibleTiles, 0, y);
            }
        }

        private void TryPlaceRandomTile(string[] possibleTiles, int x, int y)
        {
            Debug.Log($"Trying to place border tile at ({x}, {y})");

            // Перебираємо всі можливі тайли в випадковому порядку
            var shuffledTiles = possibleTiles.OrderBy(t => Random.value).ToList();

            foreach (string tileCode in shuffledTiles)
            {
                TileData tile = new TileData(tileCode);

                // Пробуємо всі можливі повороти
                for (int rotation = 0; rotation < 4; rotation++)
                {
                    if (CanPlaceTile(tile, x, y))
                    {
                        Debug.Log($"Successfully placed tile {tile.id} at ({x}, {y}) with rotation {rotation}");
                        PlaceTile(tile, x, y);
                        return;
                    }
                    tile.Rotate();
                }
            }

            // Якщо не вдалося розмістити жоден тайл, розміщуємо просте поле
            Debug.LogWarning($"Failed to place matching tile at ({x}, {y}), placing field");
            PlaceTile(new TileData("FFFF"), x, y);
        }

        public bool PlaceTile(TileData tile, int x, int y)
        {
            Debug.Log($"Attempting to place tile {tile.id} at position ({x}, {y})");

            if (!CanPlaceTile(tile, x, y))
            {
                Debug.LogWarning($"Cannot place tile: position check failed");
                return false;
            }

            Debug.Log($"Position check passed, placing tile");
            tileData[x, y] = tile;
            GameObject tileObject = tileObjects[x, y];

            if (tileObject == null)
            {
                Debug.LogError($"Tile object at ({x}, {y}) is null!");
                return false;
            }

            tileObject.SetActive(true);

            // Оновлюємо візуальне відображення
            TileView tileView = tileObject.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.UpdateView(tile);
                Debug.Log($"Updated tile view for {tile.id}");
            }
            else
            {
                Debug.LogError($"TileView component missing on tile at ({x}, {y})");
            }

            return true;
        }

        public bool CanPlaceTile(TileData tile, int x, int y)
        {
            Debug.Log($"Checking if can place tile {tile.id} at position ({x}, {y})");

            // Перевірка границь поля
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                Debug.LogWarning("Position out of bounds");
                return false;
            }

            // Перевірка чи позиція вільна
            if (tileData[x, y] != null)
            {
                Debug.LogWarning("Position already occupied");
                return false;
            }

            // Перевіряємо чи це перший тайл на полі
            bool isFirstTile = true;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (tileData[i, j] != null)
                    {
                        isFirstTile = false;
                        break;
                    }
                }
                if (!isFirstTile) break;
            }

            // Для першого тайлу в контурі
            if (isFirstTile && (x == 0 || x == width - 1 || y == 0 || y == height - 1))
            {
                return true;
            }

            // Знаходимо всі сусідні тайли
            Dictionary<Side, TileData> neighbors = new Dictionary<Side, TileData>();
            bool hasAnyNeighbor = false;

            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + GetXOffset(side);
                int newY = y + GetYOffset(side);

                if (IsValidPosition(newX, newY) && tileData[newX, newY] != null)
                {
                    neighbors[side] = tileData[newX, newY];
                    hasAnyNeighbor = true;
                }
            }

            // Якщо немає сусідів взагалі, тайл не можна розмістити
            if (!hasAnyNeighbor)
            {
                Debug.LogWarning("No adjacent tiles found");
                return false;
            }

            // Перевіряємо кожну сторону тайлу
            foreach (var neighbor in neighbors)
            {
                Side side = neighbor.Key;
                TileData adjacentTile = neighbor.Value;

                // Отримуємо типи ландшафтів, які повинні з'єднатися
                LandscapeType currentSide = tile.GetSide(side);
                LandscapeType adjacentSide = adjacentTile.GetSide(GetOppositeSide(side));

                Debug.Log($"Checking {side} side: Current tile {currentSide} vs Adjacent tile {adjacentSide}");

                // Якщо типи не співпадають, тайл не можна розмістити
                if (!IsMatchingLandscape(currentSide, adjacentSide))
                {
                    Debug.LogWarning($"Mismatch on {side} side: {currentSide} cannot connect to {adjacentSide}");
                    return false;
                }
            }

            // Якщо всі перевірки пройдені успішно, тайл можна розмістити
            Debug.Log("All connections valid - tile can be placed");
            return true;
        }

        private bool IsMatchingLandscape(LandscapeType type1, LandscapeType type2)
        {
            Debug.Log($"Comparing landscapes: {type1} with {type2}");

            bool matches = (type1, type2) switch
            {
                (LandscapeType.City, LandscapeType.City) => true,
                (LandscapeType.Road, LandscapeType.Road) => true,
                (LandscapeType.Field, LandscapeType.Field) => true,
                _ => false
            };

            Debug.Log($"Landscapes {(matches ? "match" : "don't match")}");
            return matches;
        }

        private Side GetOppositeSide(Side side)
        {
            return side switch
            {
                Side.Top => Side.Bottom,
                Side.Right => Side.Left,
                Side.Bottom => Side.Top,
                Side.Left => Side.Right,
                _ => throw new System.ArgumentException($"Invalid side: {side}")
            };
        }

        private int GetXOffset(Side dir)
        {
            // В ізометричній проекції:
            // x збільшується при русі вгору (Top)
            // x зменшується при русі вниз (Bottom)
            return dir switch
            {
                Side.Top => 1,
                Side.Bottom => -1,
                _ => 0
            };
        }

        private int GetYOffset(Side dir)
        {
            // В ізометричній проекції:
            // y збільшується при русі вліво (Left)
            // y зменшується при русі вправо (Right)
            return dir switch
            {
                Side.Left => 1,
                Side.Right => -1,
                _ => 0
            };
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public int Width => width;
        public int Height => height;

        public Vector3 GetTilePosition(int x, int y)
        {
            float xPosition = (x - y) * 0.5f;
            float yPosition = (x + y) * 0.25f;
            return new Vector3(xPosition, yPosition, 0);
        }

        public List<ValidPlacement> GetValidPlacements(TileData tile)
        {
            List<ValidPlacement> validPlacements = new List<ValidPlacement>();

            // Перевіряємо всі позиції на дошці
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Для кожної позиції перевіряємо всі можливі повороти
                    for (int rotation = 0; rotation < 4; rotation++)
                    {
                        if (CanPlaceTile(tile, x, y))
                        {
                            validPlacements.Add(new ValidPlacement
                            {
                                X = x,
                                Y = y,
                                Rotation = rotation
                            });
                        }
                        tile.Rotate();
                    }
                    // Повертаємо тайл в початкове положення
                    tile.Rotate(4 - (tile.rotationIndex % 4));
                }
            }

            return validPlacements;
        }

        public List<int> GetValidRotations(TileData tile, int x, int y)
        {
            List<int> validRotations = new List<int>();

            // Зберігаємо початковий поворот
            int initialRotation = tile.rotationIndex;

            // Перевіряємо всі можливі повороти
            for (int rotation = 0; rotation < 4; rotation++)
            {
                if (CanPlaceTile(tile, x, y))
                {
                    validRotations.Add(rotation);
                }
                tile.Rotate();
            }

            // Повертаємо тайл в початкове положення
            tile.Rotate(4 - (tile.rotationIndex % 4));
            while (tile.rotationIndex != initialRotation)
            {
                tile.Rotate();
            }

            return validRotations;
        }
    }
}
