using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.ScriptablesObjects;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
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

        public void Initialize()
        {
            structureChecker = new StructureChecker(this);
            //var debugger = gameObject.AddComponent<StructureDebugger>();
            //debugger.Initialize(structureChecker, this);
            //Random.InitState(4);
            InitializeBoard();
            var onChainBoard = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;
            char[] edgeTiles = OnChainBoardDataConverter.GetInitialEdgeState(onChainBoard.initial_edge_state);
            CreateBorder(edgeTiles);

        }

        public void OnDestroy()
        {
            structureChecker.OnDestroy();
        }

        private void InitializeBoard()
        {
            tileObjects = new GameObject[width, height];
            tileData = new TileData[width, height];
        }
        
        private void CreateBorder(char[] border)
        {
            GenerateBorderSide(new Vector2Int(0, 0), new Vector2Int(9, 0), 0, border[0..8],true);
            GenerateBorderSide(new Vector2Int(9, 0), new Vector2Int(9, 9), 3, border[8..16]);
            GenerateBorderSide(new Vector2Int(9, 9), new Vector2Int(0, 9), 2, border[16..24]);
            GenerateBorderSide(new Vector2Int(0, 9), new Vector2Int(0, 0), 1, border[24..32], true);
        }
        
        public void GenerateBorderSide(Vector2Int startPos, Vector2Int endPos, int rotationTimes, char[] border, bool swapOrderLayer = false)
        {
            string roadTile = "FFFR";
            string cityTile = "FFFC";
            string fieldTile = "FFFF";
            // eg Start (9, 9) end (9, 0)

            roadTile = TileData.GetRotatedConfig(roadTile, rotationTimes);
            cityTile = TileData.GetRotatedConfig(cityTile, rotationTimes);

            string[] tilesToSpawn = new string[8];
            for (int i = 0; i < tilesToSpawn.Length; i++)
            {
                tilesToSpawn[i] = border[i] switch {
                    'R' => roadTile,
                    'C' => cityTile,
                    'F' => fieldTile,
                    _ => fieldTile
                };
            }

            List<Vector2Int> availablePositions = new List<Vector2Int>();
            if (endPos.y != startPos.y)
            {
                for (int i = startPos.y + 1; i < endPos.y; i++)
                {
                    availablePositions.Add(new Vector2Int(startPos.x, i));
                }
                for (int i = startPos.y - 1; i > endPos.y; i--)
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
                for (int i = startPos.x - 1; i > endPos.x; i--)
                {
                    availablePositions.Add(new Vector2Int(i, startPos.y));
                }
                
            }

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
                    if (swapOrderLayer)
                    {
                        tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("RoadRenderer")
                            .GetComponent<SpriteRenderer>().sortingOrder = 20;
                    }
                }
                else if (tilesToSpawn[i] == roadTile)
                {
                    tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("BorderArc").gameObject.SetActive(true);
                    Transform borderArc = tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("BorderArc");
                    TileRotator.GetMirrorRotationStatic(borderArc, rotationTimes);
                }
            }
        }
        
        public bool PlaceTile(TileData data, int x, int y, int ownerId)
        {
            // if (!CanPlaceTile(data, x, y))
            // {
            //     CustomLogger.LogWarning($"Can't place tile {data.id} at {x}, {y}");
            //     return false;
            // }

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
            
            if( (x == 1 || x == width - 2 || y == 1 || y == height - 2) && !IsEdgeTile(x, y))
            {
                TryConnectEdgeStructure(ownerId, x, y);
            }

            return true;
        }

        private void TryConnectEdgeStructure(int owner, int x, int y)
        {
            CustomLogger.LogInfo($"TryConnectEdgeStructure at {x}, {y}");
            GameObject[] neighborsGO = new GameObject[4];
            TileData[] neighborsData = new TileData[4];
            int[,] positions = new int[4, 2] { {1, 0}, {0, -1}, {-1, 0}, {0, 1} };
            int[,] tilePositions = new int[4, 2];
            
            for (int i = 0; i < 4; i++)
            {
                int newX = x + positions[i, 0];
                int newY = y + positions[i, 1];
                tilePositions[i, 0] = newX;
                tilePositions[i, 1] = newY;
                neighborsGO[i] = tileObjects[newX, newY];
                neighborsData[i] = tileData[newX, newY];
            }
            
            for( int i = 0; i < 4; i++)
            {
                if(IsEdgeTile(tilePositions[i, 0], tilePositions[i, 1]) && neighborsGO[i] != null)
                {
                    TileGenerator tileGenerator = neighborsGO[i].GetComponent<TileGenerator>();
                    foreach (var renderer in tileGenerator.houseRenderers)
                    {
                        //renderer.sprite = tileAssets.GetHouseByReference(renderer.sprite, owner);
                        tileGenerator.RecolorHouses(owner);
                    }

                    foreach (var pin in tileGenerator.pinRenderers)
                    {
                        if (pin == null) continue;
                        pin.sprite = tileAssets.GetPinByPlayerId(owner);
                    }
                }
            }
        }
        
        private bool IsEdgeTile(int x, int y)
        {
            return x == 0 || x == width - 1 || y == 0 || y == height - 1;
        }
        
        public bool CanPlaceTile(TileData tile, int x, int y)
        {
            // Перевірка меж поля
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return false;
            }

            // Перевірка чи позиція вільна
            if (tileData[x, y] != null)
            {
                return false;
            }

            // Підрахунок кількості розміщених тайлів
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

            // Якщо карта менше 36 тайлів, дозволяємо розміщення в будь-якій позиції
            if (placedTiles < 36)
            {
                return true;
            }

            // Знаходимо всі сусідні тайли
            Dictionary<Side, TileData> neighbors = new Dictionary<Side, TileData>();
            bool hasAnyNeighbor = false;
            bool hasNonBorderNeighbor = false;
            bool hasBorderWithNonField = false;

            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + GetXOffset(side);
                int newY = y + GetYOffset(side);

                if (IsValidPosition(newX, newY) && tileData[newX, newY] != null)
                {
                    neighbors[side] = tileData[newX, newY];
                    hasAnyNeighbor = true;

                    if (IsBorderTile(newX, newY))
                    {
                        // Перевіряємо чи граничний тайл має дорогу або місто
                        LandscapeType borderSide = tileData[newX, newY].GetSide(GetOppositeSide(side));
                        if (borderSide != LandscapeType.Field)
                        {
                            hasBorderWithNonField = true;
                        }
                    }
                    else
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

            // Якщо є граничний сусід з дорогою або містом, дозволяємо розміщення
            if (hasBorderWithNonField)
            {
                // Перевіряємо відповідність типів для всіх сусідів
                foreach (var neighbor in neighbors)
                {
                    Side side = neighbor.Key;
                    TileData adjacentTile = neighbor.Value;
                    if (!IsMatchingLandscape(tile.GetSide(side), 
                        adjacentTile.GetSide(GetOppositeSide(side))))
                    {
                        return false;
                    }
                }
                return true;
            }

            // Якщо немає граничного сусіда з дорогою/містом і немає звичайного сусіда
            if (!hasNonBorderNeighbor)
            {
                return false;
            }

            // Перевіряємо кожну сторону тайла на відповідність
            foreach (var neighbor in neighbors)
            {
                Side side = neighbor.Key;
                TileData adjacentTile = neighbor.Value;

                LandscapeType currentSide = tile.GetSide(side);
                LandscapeType adjacentSide = adjacentTile.GetSide(GetOppositeSide(side));

                // Якщо сусід - граничний тайл і його сторона - поле (F), дозволяємо будь-який тип
                if (IsBorderTile(x + GetXOffset(side), y + GetYOffset(side)) && adjacentSide == LandscapeType.Field)
                {
                    continue;
                }

                // Інакше перевіряємо на відповідність типів
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

            // find all the neighboring tiles
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

                    // Check whether it is not a borderline tile
                    if (!IsBorderTile(newX, newY))
                    {
                        hasNonBorderNeighbor = true;
                    }
                }
            }
            
            if (!hasAnyNeighbor)
            {
                return;
            }
            
            if (neighbors.Count == 1 && !hasNonBorderNeighbor)
            {
                var neighbor = neighbors.First();
                if (neighbor.Value.GetSide(GetOppositeSide(neighbor.Key)) == LandscapeType.Field)
                {
                    return;
                }
            }
            
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
           // In isometric projection:
            // x increases when moving up (Top)
            // x decreases when moving down (Bottom)
            return dir switch
            {
                Side.Top => 1,
                Side.Bottom => -1,
                _ => 0
            };
        }

        public int GetYOffset(Side dir)
        {
            // In isometric projection:
            // y increases when moving to the left (Left)
            // y decreases when moving to the right (Right)
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
            // Check whether the tile is on the field border
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
        
        public void RoadContest(byte root, int winnerId, int bluePoints, int redPoints)
        {
            List<RoadStructure> connectedRoadTiles = GetConnectedRoadTiles(root);
            foreach (RoadStructure roadStructure in connectedRoadTiles)
            {
                TileData tile = GetTileData(roadStructure.tilePosition.x, roadStructure.tilePosition.y);
                if (tile != null && tile.IsRoad())
                {
                    tile.RoadStructure.OwnerId = winnerId;
                    // Можливо, тут потрібно оновити візуальне відображення доріг
                    // тільки для тих сторін, де roadStructure.roadSides[i] == true
                    
                }
                
                
                // GameObject tileObject = GetTileObject(roadStructure.tilePosition.x, roadStructure.tilePosition.y);
                // if (tileObject != null)
                // {
                //     //TileGenerator tileGenerator = tileObject.GetComponent<TileGenerator>();
                //     //tileGenerator.RecolorRoads(winnerId);
                //     // через DOTween зробити анімацію припідняття тайла, 1 секунда завися і повернути на місце
                //     Sequence sequence = DOTween.Sequence();
                //     sequence.Append(tileObject.transform.DOMoveY(tileObject.transform.position.y + 0.2f, 0.5f).SetEase(Ease.InOutSine));
                //     sequence.AppendInterval(1.5f);
                //     sequence.Append(tileObject.transform.DOMoveY(tileObject.transform.position.y, 0.5f).SetEase(Ease.InOutSine));
                //     sequence.Play();
                // }
                Vector2 centralPinPosition = ReplacePinsAndGetCentralPosition(connectedRoadTiles, winnerId);
                Vector2 offset = new Vector2(0, 0.4f);
            }
        }

        public List<RoadStructure> GetConnectedRoadTiles(byte root)
        {
            Vector2Int startPosition = OnChainBoardDataConverter.GetPositionByRoot(root);
            // Конвертуємо startSide в enum Side
            Side startSide = (Side)((root + 3) % 4);
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            List<RoadStructure> roadTiles = new List<RoadStructure>();
            //roadTiles[0].roadSides[(int)startSide] = true;

            

            // Передаємо startSide як fromSide для першого тайлу
            DfsRoadSearch(startPosition.x, startPosition.y, visited, roadTiles, startSide);
            
            return roadTiles;
        }
        
        private Vector2 ReplacePinsAndGetCentralPosition(List<RoadStructure> roadTiles, int winnerId)
        {
            List<Vector2> pinsPositions = new List<Vector2>();
            foreach (RoadStructure roadStructure in roadTiles)
            {
                GameObject tileObject = GetTileObject(roadStructure.tilePosition.x, roadStructure.tilePosition.y);
                if (tileObject != null)
                {
                    TileGenerator tileGenerator = tileObject.GetComponent<TileGenerator>();
                    for(int i = 0; i < 4; i++)
                    {
                        if (roadStructure.roadSides[i])
                        {
                            if (tileGenerator.pinRenderers[i] == null) continue;
                            pinsPositions.Add(tileGenerator.pinRenderers[i].transform.position);    
                            
                            tileGenerator.pinRenderers[i].sprite = tileAssets.GetPinByPlayerId(winnerId);
                            //tileGenerator.pinRenderers[i].transform.DOKill();
                            //Destroy(tileGenerator.pinRenderers[i].gameObject);
                        }
                    }
                }
            }
            // sort pinsPositions by x and y
            pinsPositions.Sort((a, b) => a.x.CompareTo(b.x));
            return pinsPositions[pinsPositions.Count / 2];
        }

        private void DfsRoadSearch(int x, int y, HashSet<Vector2Int> visited, List<RoadStructure> roadTiles, Side? fromSide = null)
        {
            string s = "";
            s += $"DfsRoadSearch at {x}, {y} from {fromSide} visited: \n";
            Vector2Int currentPos = new Vector2Int(x, y);

            if (visited.Contains(currentPos) || !IsValidPosition(x, y))
            {
                CustomLogger.LogWarning(s);
                return;
            }
            s += $"DfsRoadSearch at {x}, {y} is not visited and valid\n";
            
            TileData currentTile = GetTileData(x, y);
            if (currentTile == null || !currentTile.IsRoad())
            {
                CustomLogger.LogWarning(s);
                return;
            }
            s += $"DfsRoadSearch at {x}, {y} is road\n";
            
            visited.Add(currentPos);
            
            RoadStructure roadStructure = new RoadStructure 
            { 
                tilePosition = currentPos,
                roadSides = new bool[4]
            };

            // Підрахуємо кількість доріг на тайлі і позначимо їх
            int roadCount = 0;
            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                if (currentTile.GetSide(side) == LandscapeType.Road)
                {
                    roadCount++;
                    roadStructure.roadSides[(int)side] = true;
                }
            }
            s += $"DfsRoadSearch at {x}, {y} roadCount: {roadCount}\n";
            
            // Якщо дорога одинарна, потрійна або четверна - залишаємо тільки потрібну сторону
            if (roadCount == 1 || roadCount >= 3)
            {
                // Завжди використовуємо fromSide, оскільки тепер воно є і для першого тайлу
                s += $"DfsRoadSearch at {x}, {y} fromSide: {fromSide}\n";
                // Залишаємо тільки ту сторону, з якої прийшли
                for (int i = 0; i < 4; i++)
                {
                    roadStructure.roadSides[i] = (i == (int)GetOppositeSide(fromSide.Value));
                }
                
                roadTiles.Add(roadStructure);
                s += $"DfsRoadSearch at {x}, {y} roadStructure added\n";
                CustomLogger.LogWarning(s);
                return;
            }

            roadTiles.Add(roadStructure);
            
            // Перевіряємо всі сторони для подальшого пошуку
            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                if (!roadStructure.roadSides[(int)side])
                    continue;

                int newX = x + GetXOffset(side);
                int newY = y + GetYOffset(side);
                s += $"DfsRoadSearch at {x}, {y} side: {side} newX: {newX} newY: {newY}\n";
                
                TileData neighborTile = GetTileData(newX, newY);
                if (neighborTile != null && 
                    neighborTile.GetSide(GetOppositeSide(side)) == LandscapeType.Road)
                {
                    s += $"DfsRoadSearch at {x}, {y} side: {side} newX: {newX} newY: {newY} is road\n";
                    DfsRoadSearch(newX, newY, visited, roadTiles, side);
                }
            }
            CustomLogger.LogWarning(s);
        }

        public class RoadStructure
        {
            public Vector2Int tilePosition;
            public bool[] roadSides = new bool[4];
        }
    }
}
