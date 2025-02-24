using System;
using System.Collections.Generic;
using System.Linq;
using TerritoryWars.General;
using TerritoryWars.ScriptablesObjects;
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


        [Header("Roads")]
        public List<RoadPair> RoadPairs;

        [Header("Cities")]
        public List<CityData> CityData;

        [HideInInspector]
        public string TileConfig;

        public GameObject RoadPath;
        
        private int _currentHouseIndex = 0;
        
        public List<SpriteRenderer> AllCityRenderers = new List<SpriteRenderer>();
        public List<LineRenderer> AllCityLineRenderers = new List<LineRenderer>();


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
            Generate();
        }

        public void Generate()
        {
            Destroy(City);
            Destroy(Mill);
            City = null;
            Mill = null;
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
                if(Mill != null)
                    Destroy(Mill);
                Mill = Instantiate(PrefabsManager.Instance.MillPrefab, transform);
                Mill.transform.localPosition = Vector3.zero;
            }
            else
            {
                if(Mill != null)
                    Destroy(Mill);
            }
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
            List<SpriteRenderer> houseRenderers = city.GetComponentsInChildren<SpriteRenderer>()
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
                General.GameManager gameManager = General.GameManager.Instance;
                if (gameManager == null || gameManager.CurrentCharacter == null)
                {
                   playerId = Random.Range(0, 2);
                }
                else
                {
                    playerId = gameManager.CurrentCharacter.Id;
                }
                house.sprite = TileAssetsObject.GetNextHouse(playerId);
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
            SpriteRenderer[] houseRenderers = City.GetComponentsInChildren<SpriteRenderer>()
                .ToList().Where(x => x.name == "House").ToArray();
            for (int i = 0; i < houseRenderers.Length; i++)
            {
                houseRenderers[i].sprite = TileAssetsObject.GetHouseByReference(houseRenderers[i].sprite, playerId);
            }
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