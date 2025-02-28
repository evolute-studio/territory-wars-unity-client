using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TerritoryWars.General;
using TerritoryWars.ScriptablesObjects;
using TerritoryWars.Tools;
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


        [Header("Roads")] public List<RoadPair> RoadPairs;

        [Header("Cities")] public List<CityData> CityData;

        [HideInInspector] public string TileConfig;

        public GameObject RoadPath;

        private int _currentHouseIndex = 0;

        public List<SpriteRenderer> AllCityRenderers = new List<SpriteRenderer>();
        public List<LineRenderer> AllCityLineRenderers = new List<LineRenderer>();
        public List<SpriteRenderer> Pins = new List<SpriteRenderer>();
        public List<SpriteRenderer> houseRenderers;
        public SpriteRenderer[] pinRenderers;


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

        public void Generate(TileData data)
        {
            TileConfig = data.id;
            _tileData = data;
            Generate();
        }

        public void Generate()
        {
            Destroy(City);
            Destroy(Mill);
            foreach (var pin in Pins)
            {
                if(pin == null) continue;
                pin.transform.DOKill();
                Destroy(pin.gameObject);
            }
            City = null;
            Mill = null;
            Pins.Clear();
            AllCityRenderers = new List<SpriteRenderer>();
            AllCityLineRenderers = new List<LineRenderer>();

            TileRotator.ClearLists();

            GenerateRoad();
            GenerateCity();
        }

        public void Rotate()
        {
            string s = "Previous config: " + TileConfig;
            TileConfig = TileData.GetRotatedConfig(TileConfig);
            s += " Rotated config: " + TileConfig;
            Debug.Log(s);
            SetConnectorTypes();

            GenerateRoad();
            GenerateCity();
        }

        public void GenerateRoad()
        {
            string id = TileConfig;

            if (string.IsNullOrEmpty(id) || !id.Contains('R'))
            {
                RoadRenderer.sprite = null;
                return;
            }

            int roadCount = id.Count(c => c == 'R');
            int cityCount = id.Count(c => c == 'C');

            if (roadCount == 1)
            {
                for (int i = 0; i < id.Length; i++)
                {
                    char[] idReplace = id.ToCharArray();
                    if (id[i] == 'C')
                    {
                        idReplace[i] = 'R';
                        id = new string(idReplace);
                        break;
                    }
                }

                roadCount = 2;
            }


            id = id.Replace('C', 'X');
            id = id.Replace('F', 'X');
            RoadRenderer.transform.localScale = Vector3.one;
            
            if(RoadPath != null)
                Destroy(RoadPath);

            foreach (var roadPair in RoadPairs)
            {
                if (roadPair.MainConfig == id)
                {
                    RoadRenderer.sprite = roadPair.Sprite;
                    RoadPath = Instantiate(roadPair.RoadPath, RoadRenderer.transform);
                    break;
                }

                if (roadPair.MirroredConfig == id)
                {
                    RoadRenderer.sprite = roadPair.Sprite;
                    Vector3 scale = RoadRenderer.transform.localScale;
                    if (roadPair.FlipX) scale.x = -scale.x;
                    if (roadPair.FlipY) scale.y = -scale.y;
                    RoadRenderer.transform.localScale = scale;

                    RoadPath = Instantiate(roadPair.RoadPath, RoadRenderer.transform);
                    break;
                }
            }

            if (roadCount >= 3 && cityCount == 0)
            {
                if (Mill != null)
                    Destroy(Mill);
                Mill = Instantiate(PrefabsManager.Instance.MillPrefab, transform);
                Mill.transform.localPosition = Vector3.zero;
            }
            else
            {
                if (Mill != null)
                    Destroy(Mill);
            }

            GenerateRoadPins();
        }

        private void GenerateRoadPins()
        {
            pinRenderers = new SpriteRenderer[4];
            int playerId = SessionManager.Instance.CurrentTurnPlayer != null
                ? SessionManager.Instance.CurrentTurnPlayer.LocalId
                : -1;
            if (RoadPath == null)
            {
                return;
            }
            Transform[] points = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                points[i] = RoadPath.transform.GetChild(i);
            }
            CustomLogger.LogInfo("Points count: " + points.Length);
            SpriteRenderer[] pins = new SpriteRenderer[4];
            char[] id = TileConfig.ToCharArray();
            for (int i = 0; i < id.Length; i++)
            {
                RoadPath.transform.localScale = RoadPath.transform.parent.localScale;
                if (id[i] == 'R')
                {
                    CustomLogger.LogInfo("Creating pin for road " + i + " parent: " + points[i].name);
                    GameObject pin = Instantiate(PrefabsManager.Instance.PinPrefab, points[i]);
                    SpriteRenderer pinRenderer = pin.GetComponent<SpriteRenderer>();
                    pinRenderers[i] = pinRenderer;
                    pin.transform.parent = points[i];
                    // Vector3 scale = pinRenderer.transform.localScale;
                    // scale.x = pinRenderer.transform.parent.parent.parent.localScale.x;
                    // scale.y = pinRenderer.transform.parent.parent.parent.localScale.y;
                    // pinRenderer.transform.localScale = scale;
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
    

    public void GenerateCity()
        {
            string id = TileConfig;
            int cityCount = id.Count(c => c == 'C');
            int roadCount = id.Count(c => c == 'R');

            if (cityCount == 1 && roadCount == 3) { }

            else if(cityCount == 3 && roadCount == 1) { }
            else
            {
                if (string.IsNullOrEmpty(id) || !id.Contains('C')) 
                {
                    return;
                }
                
                id = id.Replace('R', 'X');
                id = id.Replace('F', 'X');
                
            }


            if (City != null)
            {
                return;
            }

            foreach (var cityData in CityData)
            {
                if (cityData.Config == id)
                {
                    City = Instantiate(cityData.CityPrefab, transform);
                    InitCity(City, cityData.Rotation);

                    return;
                }
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
            if (City == null)
            {
                return;
            }
            Debug.Log("Recoloring houses. PlayerId: " + playerId);
            houseRenderers = City.GetComponentsInChildren<SpriteRenderer>()
                .ToList().Where(x => x.name == "House").ToList();
            for (int i = 0; i < houseRenderers.Count; i++)
            {
                houseRenderers[i].gameObject.GetComponent<SpriteAnimator>().ChangeSprites(TileAssetsObject.GetHouseByReference(houseRenderers[i].gameObject.GetComponent<SpriteAnimator>().sprites, playerId));
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
            if (pinRenderers[side] == null)
            {
                return;
            }
            pinRenderers[side].sprite = TileAssetsObject.GetPinByPlayerId(playerId);
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
}