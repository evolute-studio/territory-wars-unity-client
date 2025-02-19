using System.Collections.Generic;
using System.Linq;
using TerritoryWars.ScriptablesObjects;
using TerritoryWars.Tile;
using UnityEngine;

namespace TerritoryWars.General
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
        private StructureChecker structureChecker;

        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private float tileSpacing = 0.5f;

        private GameObject[,] tileObjects;
        private TileData[,] tileData;
        
        public Dictionary<Vector2Int, TileData> PlacedTiles = new Dictionary<Vector2Int, TileData>();

        public delegate void TilePlaced(TileData tile, int x, int y);
        public event TilePlaced OnTilePlaced;

        private void Awake()
        {
            structureChecker = new StructureChecker(this);
            //var debugger = gameObject.AddComponent<StructureDebugger>();
            //debugger.Initialize(structureChecker, this);
            Random.InitState(4);
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

            string[] tilesToSpawn = new string[] { fieldTile, fieldTile, fieldTile, cityTile, fieldTile, fieldTile, roadTile, fieldTile };

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

            // Place forests only at (0,9) and (9,0), mountains at other corners
            if (startPos.x == 0 && startPos.y == 9)
            {
                PlaceTile(new TileData(fieldTile), 0, 9, -1);
                GameObject forest = Instantiate(tileAssets.ForestPrefab, transform.position, Quaternion.identity, tileObjects[0, 9].transform);
                forest.transform.localPosition = Vector3.zero;
                GameObject spawnedTile = tileObjects[0, 9];
                Destroy(spawnedTile);
            }
            else if (startPos.x == 9 && startPos.y == 0)
            {
                PlaceTile(new TileData(fieldTile), 9, 0, -1);
                GameObject forest = Instantiate(tileAssets.ForestPrefab, transform.position, Quaternion.identity, tileObjects[9, 0].transform);
                forest.transform.localPosition = Vector3.zero;
                forest.transform.localScale = new Vector3(-1, 1, 1);
                GameObject spawnedTile = tileObjects[9, 0];
                Destroy(spawnedTile);
            }
            else
            {
                // Place mountains at start and end positions
                PlaceTile(new TileData(fieldTile), startPos.x, startPos.y, -1);
                // Don't place forests, only mountains at other corners
                tileObjects[startPos.x, startPos.y].transform.Find("RoadRenderer").GetComponent<SpriteRenderer>().sprite = tileAssets.GetRandomMountain();
            }

            if (endPos != new Vector2Int(9, 0) && endPos != new Vector2Int(0, 9))
            {
                PlaceTile(new TileData(fieldTile), endPos.x, endPos.y, -1);
                tileObjects[endPos.x, endPos.y].transform.Find("RoadRenderer").GetComponent<SpriteRenderer>().sprite = tileAssets.GetRandomMountain();
            }

            for (int i = 0; i < availablePositions.Count; i++)
            {
                TileData tile = new TileData(tilesToSpawn[i]);
                PlaceTile(tile, availablePositions[i].x, availablePositions[i].y, -1);
                if (tilesToSpawn[i] == fieldTile)
                {
                    tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("RoadRenderer").GetComponent<SpriteRenderer>().sprite = tileAssets.GetRandomMountain();
                }
            }
        }

        public bool PlaceTile(TileData data, int x, int y, int ownerId)
        {
            if (!CanPlaceTile(data, x, y))
            {
                Debug.LogWarning($"Cannot place tile: position check failed");
                return false;
            }

            tileData[x, y] = data;
            data.OwnerId = ownerId;
            GameObject tile = Instantiate(tilePrefab, GetTilePosition(x, y), Quaternion.identity, transform);
            tile.name += $"_{x}_{y}";
            tile.GetComponent<TileGenerator>().Generate(data);
            tile.GetComponent<TileView>().UpdateView(data);
            tileObjects[x, y] = tile;
            PlacedTiles[new Vector2Int(x, y)] = data;
            OnTilePlaced?.Invoke(data, x, y);
            CheckConnections(data, x, y);

            return true;
        }

        public bool CanPlaceTile(TileData tile, int x, int y)
        {
            // Checking the boundaries of the field
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return false;
            }

            // Checking or position is free
            if (tileData[x, y] != null)
            {
                return false;
            }

            // We count the number of tiles placed
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

            // If the map is less than 36 tile, we allow placement in any position
            if (placedTiles < 36)
            {
                return true;
            }

            // Знаходимо всі сусідні тайли
            Dictionary<Side, TileData> neighbors = new Dictionary<Side, TileData>();
            bool hasAnyNeighbor = false;
            bool hasNonBorderNeighbor = false;

            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + GetXOffset(side);
                int newY = y + GetYOffset(side);

                if (IsValidPosition(newX, newY) && tileData[newX, newY] != null)
                {
                    neighbors[side] = tileData[newX, newY];
                    hasAnyNeighbor = true;

                    // Перевіряємо чи це не граничний тайл
                    if (!IsBorderTile(newX, newY))
                    {
                        hasNonBorderNeighbor = true;
                    }
                }
            }

            // Якщо немає сусідів взагалі
            if (!hasAnyNeighbor)
            {
                return false;
            }

            // Якщо є тільки один сусід і це граничний тайл
            if (neighbors.Count == 1 && !hasNonBorderNeighbor)
            {
                var neighbor = neighbors.First();
                // Перевіряємо, чи сторона граничного тайла - поле
                if (neighbor.Value.GetSide(GetOppositeSide(neighbor.Key)) == LandscapeType.Field)
                {
                    return false;
                }
            }

            // Перевіряємо кожну сторону тайлу на відповідність
            foreach (var neighbor in neighbors)
            {
                Side side = neighbor.Key;
                TileData adjacentTile = neighbor.Value;

                LandscapeType currentSide = tile.GetSide(side);
                LandscapeType adjacentSide = adjacentTile.GetSide(GetOppositeSide(side));

                if (!IsMatchingLandscape(currentSide, adjacentSide))
                {
                    return false;
                }
            }

            return true;
        }

        public void CheckConnections(TileData tile, int x, int y)
        {
            // Checking the boundaries of the field
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            // Знаходимо всі сусідні тайли
            Dictionary<Side, TileData> neighbors = new Dictionary<Side, TileData>();
            bool hasAnyNeighbor = false;
            bool hasNonBorderNeighbor = false;

            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + GetXOffset(side);
                int newY = y + GetYOffset(side);

                if (IsValidPosition(newX, newY) && tileData[newX, newY] != null)
                {
                    neighbors[side] = tileData[newX, newY];
                    hasAnyNeighbor = true;

                    // Перевіряємо чи це не граничний тайл
                    if (!IsBorderTile(newX, newY))
                    {
                        hasNonBorderNeighbor = true;
                    }
                }
            }

            // Якщо немає сусідів взагалі
            if (!hasAnyNeighbor)
            {
                return;
            }

            // Якщо є тільки один сусід і це граничний тайл
            if (neighbors.Count == 1 && !hasNonBorderNeighbor)
            {
                var neighbor = neighbors.First();
                // Перевіряємо, чи сторона граничного тайла - поле
                if (neighbor.Value.GetSide(GetOppositeSide(neighbor.Key)) == LandscapeType.Field)
                {
                    return;
                }
            }

            // Перевіряємо кожну сторону тайлу на відповідність
            foreach (var neighbor in neighbors)
            {
                Side side = neighbor.Key;
                TileData adjacentTile = neighbor.Value;

                LandscapeType currentSide = tile.GetSide(side);
                LandscapeType adjacentSide = adjacentTile.GetSide(GetOppositeSide(side));
                if (!IsMatchingLandscape(currentSide, adjacentSide))
                {
                    continue;
                }
                structureChecker.CreateStructures(tile, x, y);
                structureChecker.CreateStructures(adjacentTile, x + GetXOffset(side), y + GetYOffset(side));
                if (currentSide == LandscapeType.City && adjacentSide == LandscapeType.City)
                {
                    structureChecker.UnionStructures(tile.CityStructure, adjacentTile.CityStructure);
                }

                if (currentSide == LandscapeType.Road && adjacentSide == LandscapeType.Road)
                {
                    structureChecker.UnionStructures(tile.RoadStructure, adjacentTile.RoadStructure);
                }
            }
        }
        

        private bool IsMatchingLandscape(LandscapeType type1, LandscapeType type2)
        {
            bool matches = (type1, type2) switch
            {
                (LandscapeType.City, LandscapeType.City) => true,
                (LandscapeType.Road, LandscapeType.Road) => true,
                (LandscapeType.Field, LandscapeType.Field) => true,
                _ => false
            };

            return matches;
        }

        public Side GetOppositeSide(Side side)
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

        public int GetXOffset(Side dir)
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

        public int GetYOffset(Side dir)
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

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public bool IsBorderTile(int x, int y)
        {
            // Перевіряємо чи тайл знаходиться на границі поля
            return x == 0 || x == width - 1 || y == 0 || y == height - 1;
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

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
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

        public GameObject GetTileObject(int x, int y)
        {
            return tileObjects[x, y];
        }

        public TileData GetTileData(int x, int y)
        {
            if (!IsValidPosition(x, y)) return null;
            return tileData[x, y];
        }
    }
}
