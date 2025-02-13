using System;
using System.Collections.Generic;
using System.Linq;
using TerritoryWars.ScriptablesObjects;
using UnityEngine;

namespace TerritoryWars.Tile
{
    public class TileGenerator : MonoBehaviour
    {
        public TileConnector[] connectors;
        public SpriteRenderer RoadRenderer;
        public TileAssetsObject TileAssetsObject;
        
        public TileRotator TileRotator;
        private GameObject city;


        [Header("Roads")]
        public List<RoadPair> RoadPairs;
        
        [Header("Cities")]
        public List<CityData> CityData;

        [HideInInspector]
        public string TileConfig;
        
        public GameObject RoadPath;
        
        
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
            Destroy(city);
            city = null;
            
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
            id = id.Replace('C', 'R');
            id = id.Replace('F', 'X');
            RoadRenderer.flipX = false;
            RoadRenderer.flipY = false;
            
            foreach (var roadPair in RoadPairs)
            {
                if (roadPair.MainConfig == id)
                {
                    RoadRenderer.sprite = roadPair.Sprite;
                    RoadPath = Instantiate(roadPair.RoadPath, transform);
                    return;
                }
                if (roadPair.MirroredConfig == id)
                {
                    RoadRenderer.sprite = roadPair.Sprite;
                    RoadRenderer.flipX = roadPair.FlipX;
                    RoadRenderer.flipY = roadPair.FlipY;
                    RoadPath = Instantiate(roadPair.RoadPath, transform);
                    return;
                }
            }
        }

        public void GenerateCity()
        {
            string id = TileConfig;
            if (string.IsNullOrEmpty(id) || !id.Contains('C'))
            {
                return;
            }
            id = id.Replace('R', 'X');
            id = id.Replace('F', 'X');
            
            if (city != null)
            {
                return;
            }

            foreach (var cityData in CityData)
            {
                if (cityData.Config == id)
                {
                    city = Instantiate(cityData.CityPrefab, transform);
                    InitCity(city, cityData.Rotation);
                    
                    return;
                }
            }
        }
        
        public void InitCity(GameObject city, int index)
        {
            // find in children game objects with sprite renderer and name House
            List<SpriteRenderer> houseRenderers = city.GetComponentsInChildren<SpriteRenderer>()
                .ToList().Where(x => x.name == "House").ToList();
            Transform arc = city.transform.Find("Arc");
            TerritoryFiller territoryFiller = city.GetComponentInChildren<TerritoryFiller>();
            FencePlacer fencePlacer = city.GetComponentInChildren<FencePlacer>();
            List<Transform> pillars = fencePlacer.pillars;

            foreach (var house in houseRenderers)
            {
                house.sprite = TileAssetsObject.GetNextHouse();
                TileRotator.SimpleRotation(house.transform, index);
                TileRotator.SimpleRotationObjects.Add(house.transform);
            }

            foreach (var pillar in pillars)
            {
                TileRotator.SimpleRotation(pillar, index);
                TileRotator.SimpleRotationObjects.Add(pillar);
            }
            
            if (arc != null)
            {
                TileRotator.MirrorRotation(arc, index);
                TileRotator.MirrorRotationObjects.Add(arc);
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