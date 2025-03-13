using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NUnit.Framework;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.ScriptablesObjects;
using TerritoryWars.Tools;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TerritoryWars.Tile
{
    public class TileGenerator : MonoBehaviour
    {
        public TileConnector[] connectors;
        public SpriteRenderer RoadRenderer;
        public TileAssetsObject TileAssetsObject;

        public TileJokerAnimator TileJokerAnimator;

        public TileRotator TileRotator;
        public GameObject City { get; private set; }
        public GameObject Mill { get; private set; }

        private TileData _tileData;

        [Header("Tile Prefabs")] public List<TilePrefab> TilePrefabs;

        [Header("Roads")] public List<RoadPair> RoadPairs;

        [Header("Cities")] public List<CityData> CityData;

        [HideInInspector] public string TileConfig;

        private int _currentHouseIndex = 0;

        public List<SpriteRenderer> AllCityRenderers = new List<SpriteRenderer>();
        public List<LineRenderer> AllCityLineRenderers = new List<LineRenderer>();
        public List<SpriteRenderer> Pins = new List<SpriteRenderer>();
        public List<SpriteRenderer> houseRenderers;
        public SpriteRenderer[] PinRenderers;

        private byte _rotation;
        private bool _isTilePlacing;
        private Vector2Int _placingTilePosition;

        public GameObject CurrentTileGO { get; private set; }


        public void Start() => Initialize();

        public void Initialize()
        {
            TileRotator.OnRotation.AddListener(Rotate);
        }

        public void SetConnectorTypes()
        {
            for (int i = 0; i < connectors.Length; i++)
            {
                connectors[i].SetLandscape(TileData.CharToLandscape(TileConfig[i]));
            }
        }

        public void Generate(TileData data, bool isTilePlacing = false, Vector2Int placingTilePosition = default)
        {
            TileConfig = data.id;
            _tileData = data;
            _isTilePlacing = isTilePlacing;
            _placingTilePosition = placingTilePosition;
            Generate();
        }

        public void Generate()
        {
            Destroy(CurrentTileGO);
            CurrentTileGO = null;
            
            foreach (var pin in Pins)
            {
                if(pin == null) continue;
                pin.transform.DOKill();
                Destroy(pin.gameObject);
            }
            Pins.Clear();
            AllCityRenderers = new List<SpriteRenderer>();
            AllCityLineRenderers = new List<LineRenderer>();

            TileRotator.ClearLists();

            var tileConfig = OnChainBoardDataConverter.GetTypeAndRotation(TileConfig);
            string id = OnChainBoardDataConverter.TileTypes[tileConfig.Item1];
            _rotation = (byte)((tileConfig.Item2 + 1) % 4);
            
            foreach (var tile in TilePrefabs)
            {
                if (tile.Config == id)
                {
                    CurrentTileGO = Instantiate(tile.TilePrefabGO, transform);
                    InitializeTile();
                    break;
                }
            }
        }

        public void Rotate()
        {
            string s = "Previous config: " + TileConfig;
            TileConfig = TileData.GetRotatedConfig(TileConfig);
            s += " Rotated config: " + TileConfig;
            Debug.Log(s);
            SetConnectorTypes();
            
            InitializeTile();
            
        }

        public void GenerateRoad()
        {
            // string id = TileConfig;
            //
            // if (string.IsNullOrEmpty(id) || !id.Contains('R'))
            // {
            //     RoadRenderer.sprite = null;
            //     return;
            // }
            //
            // int roadCount = id.Count(c => c == 'R');
            // int cityCount = id.Count(c => c == 'C');
            //
            // if (roadCount == 1)
            // {
            //     for (int i = 0; i < id.Length; i++)
            //     {
            //         char[] idReplace = id.ToCharArray();
            //         if (id[i] == 'C')
            //         {
            //             idReplace[i] = 'R';
            //             id = new string(idReplace);
            //             break;
            //         }
            //     }
            //
            //     roadCount = 2;
            // }
            //
            //
            // id = id.Replace('C', 'X');
            // id = id.Replace('F', 'X');
            // RoadRenderer.transform.localScale = Vector3.one;
            //
            // if(RoadPath != null)
            //     Destroy(RoadPath);
            //
            // foreach (var roadPair in RoadPairs)
            // {
            //     if (roadPair.MainConfig == id)
            //     {
            //         RoadRenderer.sprite = roadPair.Sprite;
            //         RoadPath = Instantiate(roadPair.RoadPath, RoadRenderer.transform);
            //         break;
            //     }
            //
            //     if (roadPair.MirroredConfig == id)
            //     {
            //         RoadRenderer.sprite = roadPair.Sprite;
            //         Vector3 scale = RoadRenderer.transform.localScale;
            //         if (roadPair.FlipX) scale.x = -scale.x;
            //         if (roadPair.FlipY) scale.y = -scale.y;
            //         RoadRenderer.transform.localScale = scale;
            //
            //         RoadPath = Instantiate(roadPair.RoadPath, RoadRenderer.transform);
            //         break;
            //     }
            // }
            //
            // if (roadCount >= 3 && cityCount == 0)
            // {
            //     if (Mill != null)
            //         Destroy(Mill);
            //     Mill = Instantiate(PrefabsManager.Instance.MillPrefab, transform);
            //     Mill.transform.localPosition = Vector3.zero;
            // }
            // else
            // {
            //     if (Mill != null)
            //         Destroy(Mill);
            // }
            //
            // GenerateRoadPins();
        }

        private void GenerateRoadPins(Transform[] points)
        {
            PinRenderers = new SpriteRenderer[4];
            int playerId = SessionManager.Instance.CurrentTurnPlayer != null
                ? SessionManager.Instance.CurrentTurnPlayer.LocalId
                : -1;
            
            CustomLogger.LogInfo("Points count: " + points.Length);
            
            SpriteRenderer[] pins = new SpriteRenderer[4];
            char[] id = TileConfig.ToCharArray();
            for (int i = 0; i < id.Length; i++)
            {
                if (id[i] == 'R')
                {
                    CustomLogger.LogInfo("Creating pin for road " + i + " parent: " + points[i].name);
                    GameObject pin = Instantiate(PrefabsManager.Instance.PinPrefab, points[i]);
                    SpriteRenderer pinRenderer = pin.GetComponent<SpriteRenderer>();
                    PinRenderers[i] = pinRenderer;
                    pin.transform.parent = points[i];
                    pin.GetComponentInChildren<TextMeshPro>().text = GetRoadPoints(TileConfig).ToString();
                    pinRenderer.transform.localPosition = Vector3.zero;
                    pin.transform.DOLocalMoveY(0.035f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
                    if (_tileData.OwnerId == -1) playerId = -1;
                    pinRenderer.sprite = TileAssetsObject.GetPinByPlayerId(playerId);
                    pins[i] = pinRenderer;
                }
            }
            //_tileData.RoadsPin = pins;
            Pins.AddRange(pins);
            
        }

        private int GetRoadPoints(string config)
        {
            int roadCount = TileConfig.Count(c => c == 'R');
    
            bool isCRCR = config == "CRCR" || config == "RCRC";
            if (isCRCR) return 1;
            if (roadCount == 2) return 2;
            return 1;
        }
    

        public void GenerateCity()
        {
            // string id = TileConfig;
            // int cityCount = id.Count(c => c == 'C');
            // int roadCount = id.Count(c => c == 'R');
            //
            // if (cityCount == 1 && roadCount == 3) { }
            //
            // else if(cityCount == 3 && roadCount == 1) { }
            // else
            // {
            //     if (string.IsNullOrEmpty(id) || !id.Contains('C')) 
            //     {
            //         return;
            //     }
            //     
            //     id = id.Replace('R', 'X');
            //     id = id.Replace('F', 'X');
            //     
            // }
            //
            //
            // if (City != null)
            // {
            //     return;
            // }
            //
            // foreach (var cityData in CityData)
            // {
            //     if (cityData.Config == id)
            //     {
            //         City = Instantiate(cityData.CityPrefab, transform);
            //         InitCity(City, cityData.Rotation);
            //
            //         return;
            //     }
            // }
        }

        public void InitializeTile()
        {
            TileRenderers tileRenderers = CurrentTileGO.GetComponent<TileRenderers>();
            
            var tileConfig = OnChainBoardDataConverter.GetTypeAndRotation(TileConfig);
            string id = OnChainBoardDataConverter.TileTypes[tileConfig.Item1];
            _rotation = (byte)((tileConfig.Item2 + 1) % 4);
            CurrentTileGO.GetComponent<TileRotator>().RotateTile((_rotation + 3) % 4);
            
            AllCityRenderers = new List<SpriteRenderer>();
            AllCityLineRenderers = new List<LineRenderer>();
            
            houseRenderers = tileRenderers.HouseRenderers;
            List<SpriteRenderer> arcRenderers = tileRenderers.ArcRenderers;
            TerritoryFiller territoryFiller = tileRenderers.TileTerritoryFiller;
            FencePlacer fencePlacer = tileRenderers.TileFencePlacer;
            List<Transform> pillars = null;
            Transform[] pins = tileRenderers.PinsPositions;
            if (fencePlacer != null)
            {
                pillars = fencePlacer.pillars;
            }

            if (pillars != null)
            {
                List<SpriteRenderer> pillarsRenderers = pillars.Select(x => x.GetComponent<SpriteRenderer>()).ToList();
                AllCityRenderers.AddRange(pillarsRenderers);
            }

            if (houseRenderers != null)
            { 
                AllCityRenderers.AddRange(houseRenderers);
                TileAssetsObject.BackIndex(houseRenderers.Count);
                
                foreach (var house in houseRenderers)
                {
                    int playerId = 0;
                    General.SessionManager sessionManager = General.SessionManager.Instance;
                    if (sessionManager == null || sessionManager.CurrentTurnPlayer == null)
                    {
                        playerId = Random.Range(0, 2);
                    }
                    else
                    {
                        playerId = sessionManager.CurrentTurnPlayer.LocalId;
                    }

                    if (_tileData.OwnerId == -1) playerId = -1;
                    int cityCount = TileConfig.Count(c => c == 'C');
                    house.gameObject.GetComponent<SpriteAnimator>()
                        .ChangeSprites(TileAssetsObject.GetNextHouse(playerId, cityCount == 1 ? true : false));
                }
            }

            if (arcRenderers != null)
            {
                AllCityRenderers.AddRange(arcRenderers);
            }

            
            if (fencePlacer != null)
            {
                AllCityLineRenderers.Add(fencePlacer.lineRenderer);
                fencePlacer.PlaceFence();
            }

            
            if (territoryFiller != null)
            {
                territoryFiller.PlaceTerritory();
            }
            
            if (pins != null && pins.Length > 0)
            {
                GenerateRoadPins(pins);
            }

            if (SessionManager.Instance.TileSelector.selectedPosition != null || _isTilePlacing)
            {
                FencePlacerForCloserToBorderCity(SessionManager.Instance.Board.CheckCityTileSidesToBorder(
                    _placingTilePosition.x,
                    _placingTilePosition.y), tileRenderers);
                
                MinePlaceForCloserToBorderRoad(SessionManager.Instance.Board.CheckRoadTileSidesToBorder(_placingTilePosition.x,
                    _placingTilePosition.y));
            }
        }

        public void InitCity(GameObject city, int index)
        {
            AllCityRenderers = new List<SpriteRenderer>();
            AllCityLineRenderers = new List<LineRenderer>();
            
            
            // find in children game objects with sprite renderer and name House
            houseRenderers = city.GetComponentsInChildren<SpriteRenderer>()
                .ToList().Where(x => x.name == "House").ToList();
            Transform arc = city.transform.Find("Arc");
            List<SpriteRenderer> arcRenderers = city.GetComponentsInChildren<SpriteRenderer>().ToList().Where(x => x.name == "Arc").ToList();
            TerritoryFiller territoryFiller = city.GetComponentInChildren<TerritoryFiller>();
            FencePlacer fencePlacer = city.GetComponentInChildren<FencePlacer>();
            List<Transform> pillars = fencePlacer.pillars;
            List<SpriteRenderer> pillarsRenderers = pillars.Select(x => x.GetComponent<SpriteRenderer>()).ToList();
            
            AllCityRenderers.AddRange(houseRenderers);
            AllCityRenderers.AddRange(arcRenderers);
            AllCityRenderers.AddRange(pillarsRenderers);
            AllCityLineRenderers.Add(fencePlacer.lineRenderer);
            
            
            
            TileAssetsObject.BackIndex(houseRenderers.Count);
            
            foreach (var house in houseRenderers)
            {
                int playerId = 0;
                General.SessionManager sessionManager = General.SessionManager.Instance;
                if (sessionManager == null || sessionManager.CurrentTurnPlayer == null)
                {
                   playerId = Random.Range(0, 2);
                }
                else
                {
                    playerId = sessionManager.CurrentTurnPlayer.LocalId;
                }

                if (_tileData.OwnerId == -1) playerId = -1;
                house.gameObject.GetComponent<SpriteAnimator>().ChangeSprites(TileAssetsObject.GetNextHouse(playerId));
                TileRotator.SimpleRotation(house.transform, index);
                TileRotator.SimpleRotationObjects.Add(house.transform);
            }

            foreach (var arcs in arcRenderers)
            {
                TileRotator.MirrorRotation(arcs.transform, index);
                TileRotator.MirrorRotationObjects.Add(arcs.transform);
            }

            foreach (var pillar in pillars)
            {
                TileRotator.SimpleRotation(pillar, index);
                TileRotator.SimpleRotationObjects.Add(pillar);
            }

            LineRenderer territoryLineRenderer = territoryFiller.GetComponent<LineRenderer>();
            if (territoryLineRenderer != null)
            {
                TileRotator.LineRotation(territoryLineRenderer, index);
                TileRotator.LineRenderers.Add(territoryLineRenderer);
            }

            territoryFiller.PlaceTerritory();
            fencePlacer.PlaceFence();
            TileRotator.OnRotation.AddListener(territoryFiller.PlaceTerritory);
            TileRotator.OnRotation.AddListener(fencePlacer.PlaceFence);
        }
        
        public void RecolorHouses(int playerId)
        {
            if (CurrentTileGO.GetComponent<TileRenderers>().HouseRenderers == null)
            {
                return;
            }
            Debug.Log("Recoloring houses. PlayerId: " + playerId);
            houseRenderers = CurrentTileGO.GetComponentsInChildren<SpriteRenderer>()
                .ToList().Where(x => x.name == "House").ToList();
            for (int i = 0; i < houseRenderers.Count; i++)
            {
                houseRenderers[i].gameObject.GetComponent<SpriteAnimator>().ChangeSprites(TileAssetsObject.GetHouseByReference(houseRenderers[i].gameObject.GetComponent<SpriteAnimator>().sprites, playerId));
            }
        }

        public void FencePlacerForCloserToBorderCity(List<Side> closerSides, TileRenderers tileRenderers)
        {
            if (closerSides == null || tileRenderers.HouseRenderers == null) return;

            foreach (var side in closerSides)
            {
                if(_tileData.GetSide(side) != LandscapeType.City) continue;
                
                GameObject fence = tileRenderers.CloserToBorderFences.Find(x => x.Side == side).Fence;
                fence.SetActive(true);
                fence.GetComponent<FencePlacer>().PlaceFence();
            }
        }

        public void MinePlaceForCloserToBorderRoad(List<Board.MineTileInfo> closerSides)
        {
            if (closerSides == null || !_tileData.IsRoad()) return;

            foreach (var side in closerSides)
            {
                if(_tileData.GetSide(side.Direction) != LandscapeType.Road) continue;

                foreach (var prefab in PrefabsManager.Instance.MineEnviromentTiles)
                {
                    if (prefab.Direction == side.Direction)
                    {
                        GameObject mine = Instantiate(prefab.MineTile, side.Position, Quaternion.identity);
                        side.Tile.SetActive(false);
                    }
                }
            }
        }
        
        public void RecolorPins(int playerId)
        {
            if (Pins.Count == 0)
            {
                return;
            }
            foreach (var pin in Pins)
            {
                if (pin == null) continue;
                pin.sprite = TileAssetsObject.GetPinByPlayerId(playerId);
            }
        }
        
        public void RecolorPinOnSide(int playerId, int side)
        {
            if (Pins.Count == 0)
            {
                return;
            }
            if (PinRenderers[side] == null)
            {
                return;
            }
            PinRenderers[side].sprite = TileAssetsObject.GetPinByPlayerId(playerId);
        }
    }

    [Serializable]
    public class RoadPair
    {
        public string MainConfig;
        public string MirroredConfig;
        public Sprite Sprite;
        public bool FlipX;
        public bool FlipY;
        public GameObject RoadPath;
    }

    [Serializable]
    public class CityData
    {
        public string Config;
        public int Rotation;
        public GameObject CityPrefab;
    }

    [Serializable]
    public class TilePrefab
    {
        public string Config;
        public GameObject TilePrefabGO;
    }
}