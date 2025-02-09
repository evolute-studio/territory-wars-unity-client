using System;
using System.Collections.Generic;
using TerritoryWars.ScriptablesObjects;
using UnityEngine;

namespace TerritoryWars.Tile
{
    public class TileGenerator : MonoBehaviour
    {
        public TileConnector[] connectors;
        public SpriteRenderer RoadRenderer;
        public TileAssetsObject TileAssetsObject;
        public Transform CityPrefab;
        
        public TileRotator TileRotator;
        private List<GameObject> cities = new List<GameObject>();


        [Header("Roads")]
        public List<RoadPair> RoadPairs;

        [HideInInspector]
        public string TileConfig;
        
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

        public void Generate()
        {
            foreach (var city in cities)
            {
                Destroy(city);
            }
            cities.Clear();
            
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
            id = id.Replace('C', 'X');
            id = id.Replace('F', 'X');
            foreach (var roadPair in RoadPairs)
            {
                if (roadPair.MainConfig == id)
                {
                    RoadRenderer.sprite = roadPair.Sprite;
                    RoadRenderer.flipX = false;
                    return;
                }
                if (roadPair.MirroredConfig == id)
                {
                    RoadRenderer.sprite = roadPair.Sprite;
                    RoadRenderer.flipX = true;
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

            if (cities.Count > 0) return;
            for (int i = 0; i < id.Length; i++)
            {
                if (id[i] == 'C')
                {
                    Transform city = Instantiate(CityPrefab, transform.position, Quaternion.identity, transform);
                    InitCity(city.gameObject, i);
                    cities.Add(city.gameObject);
                }
            }

            
        }
        
        public void InitCity(GameObject city, int index)
        {
            SpriteRenderer[] cityRenderers = city.GetComponentsInChildren<SpriteRenderer>();
            foreach (var cityRenderer in cityRenderers)
            {
                cityRenderer.sprite = TileAssetsObject.GetRandomHouse();
                TileRotator.SimpleRotation(cityRenderer.transform, index);
                TileRotator.SimpleRotationObjects.Add(cityRenderer.transform);
            }
            LineRenderer lineRenderer = city.GetComponentInChildren<LineRenderer>();
            if (lineRenderer != null)
            {
                TileRotator.LineRotation(lineRenderer, index);
                TileRotator.LineRenderers.Add(lineRenderer);
            }
            FencePlacer fencePlacer = city.GetComponentInChildren<FencePlacer>();
            fencePlacer.PlaceFence();
            TileRotator.OnRotation.AddListener(fencePlacer.PlaceFence);
        }



    }

    [Serializable]
    public class RoadPair
    {
        public string MainConfig;
        public string MirroredConfig;
        public Sprite Sprite;
    }
}