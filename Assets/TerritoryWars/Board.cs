using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerritoryWars.ScriptablesObjects;
using TerritoryWars.Tile;

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
        public TileAssetsObject tileAssets;
        
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private float tileSpacing = 0.5f;

        private GameObject[,] tileObjects;
        private TileData[,] tileData;

        private void Awake()
        {
            Random.InitState(1);
            InitializeBoard();
            CreateRandomBorder();
        }

        private void InitializeBoard()
        {
            tileObjects = new GameObject[width, height];
            tileData = new TileData[width, height];
        }

        private void CreateRandomBorder()
        {
            GenerateBorderSide(new Vector2Int(9, 0), new Vector2Int(9, 9), 0, true);
            GenerateBorderSide(new Vector2Int(0, 0), new Vector2Int(9, 0), 1, false);
            GenerateBorderSide(new Vector2Int(0, 0), new Vector2Int(0, 9), 2, true);
            GenerateBorderSide(new Vector2Int(0, 9), new Vector2Int(9, 9), 3, false);



        }

        public void GenerateBorderSide(Vector2Int startPos, Vector2Int endPos, int rotationTimes, bool isHorizontal)
        {
            string roadTile = "RFRF";
            string cityTile = "FFCF";
            string fieldTile = "FFFF";
            // наприклад start (9, 9) end (9, 0)

            roadTile = TileData.GetRotatedConfig(roadTile, rotationTimes);
            cityTile = TileData.GetRotatedConfig(cityTile, rotationTimes);

            string[] tilesToSpawn = new string[] { roadTile, fieldTile, fieldTile,  cityTile,  fieldTile, fieldTile, roadTile, fieldTile };

            List<Vector2Int> availablePositions = new List<Vector2Int>();
            if (isHorizontal)
            {
                for (int i = startPos.y + 1; i < endPos.y; i++)
                {
                    availablePositions.Add(new Vector2Int(startPos.x, i));
                }
            }
            else
            {
                for (int i = startPos.x + 1; i < endPos.x; i++)
                {
                    availablePositions.Add(new Vector2Int(i, startPos.y));
                }

            }
            // shuffle availablePositions
            availablePositions = availablePositions.OrderBy(x => Random.Range(0, int.MaxValue)).ToList();

            PlaceTile(new TileData(fieldTile), startPos.x, startPos.y);
            PlaceTile(new TileData(fieldTile), endPos.x, endPos.y);
            tileObjects[startPos.x, startPos.y].transform.Find("RoadRenderer").GetComponent<SpriteRenderer>().sprite = tileAssets.GetRandomMountain();
            tileObjects[endPos.x, endPos.y].transform.Find("RoadRenderer").GetComponent<SpriteRenderer>().sprite = tileAssets.GetRandomMountain();
            // place 3 tiles
            for (int i = 0; i < availablePositions.Count; i++)
            {
                PlaceTile(new TileData(tilesToSpawn[i]), availablePositions[i].x, availablePositions[i].y);
                if (tilesToSpawn[i] == fieldTile)
                {
                    tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("RoadRenderer").GetComponent<SpriteRenderer>().sprite = tileAssets.GetRandomMountain();
                }
                
            }

        }

        private void PlaceBorderSide(int start, int pos, int length, string[] roadTile, string[] cityTile, int baseRotation, bool isHorizontal)
        {
            // Створюємо список доступних позицій (без крайніх позицій)
            List<int> availablePositions = new List<int>();
            for (int i = 1; i < length - 1; i++)
            {
                availablePositions.Add(i);
            }

            // Вибираємо 3 випадкові позиції для тайлів
            List<int> selectedPositions = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, availablePositions.Count);
                selectedPositions.Add(availablePositions[randomIndex]);
                availablePositions.RemoveAt(randomIndex);
            }
            selectedPositions.Sort(); // Сортуємо для послідовного розміщення

            // Випадково вибираємо позицію для міста серед трьох вибраних
            int cityIndex = Random.Range(0, 3);

            // Розміщуємо тайли
            for (int i = 0; i < 3; i++)
            {
                int position = selectedPositions[i];
                int x = isHorizontal ? start + position : start;
                int y = isHorizontal ? pos : start + position;

                TileData tile;
                if (i == cityIndex)
                {
                    // Розміщуємо місто
                    tile = new TileData(cityTile[0]);
                }
                else
                {
                    // Розміщуємо дорогу
                    tile = new TileData(roadTile[0]);
                }

                // Повертаємо тайл потрібною стороною до центру
                for (int r = 0; r < baseRotation; r++)
                {
                    tile.Rotate();
                }

                PlaceTile(tile, x, y);
            }
        }

        public bool PlaceTile(TileData data, int x, int y)
        {
            Debug.Log($"Attempting to place tile {data.id} at position ({x}, {y})");

            if (!CanPlaceTile(data, x, y))
            {
                Debug.LogWarning($"Cannot place tile: position check failed");
                return false;
            }

            Debug.Log($"Position check passed, placing tile");
            tileData[x, y] = data;
            GameObject tile = Instantiate(tilePrefab, GetTilePosition(x, y), Quaternion.identity, transform);
            tile.name += $"_{x}_{y}";
            tile.GetComponent<TileGenerator>().Generate(data);
            tile.GetComponent<TileView>().UpdateView(data);
            tileObjects[x, y] = tile;

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

            // Підраховуємо кількість розміщених тайлів
            int placedTiles = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (tileData[i, j] != null)
                    {
                        placedTiles++;
                    }
                }
            }

            // Якщо на карті менше 36 тайлів, дозволяємо розміщення в будь-якій позиції
            if (placedTiles < 36)
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
            float xPosition = (x - y) * tileSpacing;
            float yPosition = (x + y) * (tileSpacing / 2);
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
