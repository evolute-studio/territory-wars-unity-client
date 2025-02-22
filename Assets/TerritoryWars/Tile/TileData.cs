using System.Collections.Generic;
using TerritoryWars.General;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.Tile
{

    [System.Serializable]
    public class TileData
    {
        public Structure CityStructure;
        public Structure RoadStructure;
        public string id;
        private char[] sides;
        public int rotationIndex = 0;
        public int OwnerId = -1;

        public TileData(string tileCode)
        {
            sides = tileCode.ToCharArray();
            UpdateId();
        }

        private void UpdateId()
        {
            // Створюємо новий id на основі поточного повороту
            char[] rotatedSides = new char[4];
            for (int i = 0; i < 4; i++)
            {
                // Для повороту за годинниковою стрілкою:
                // Якщо rotationIndex = 1, то:
                // Top(0) -> Right(1)
                // Right(1) -> Bottom(2)
                // Bottom(2) -> Left(3)
                // Left(3) -> Top(0)
                int sourceIndex = (i - rotationIndex + 4) % 4;
                rotatedSides[i] = sides[sourceIndex];
            }
            id = new string(rotatedSides);
        }

        public void Rotate(int times = 1)
        {
            // Додаємо поворот за годинниковою стрілкою
            rotationIndex = (rotationIndex + times) % 4;
            UpdateId();
        }

        public LandscapeType GetSide(Side side)
        {
            // Враховуємо поворот при отриманні типу сторони
            int index = ((int)side - rotationIndex + 4) % 4;
            return CharToLandscape(sides[index]);
        }
        
        public void SetCityStructure(Structure structure)
        {
            this.CityStructure = structure;
            CityStructure.OwnerId = structure.OwnerId;
        }
        
        public void SetRoadStructure(Structure structure)
        {
            this.RoadStructure = structure;
        }
        
        public void SetCityOwner(int playerId)
        {
            OwnerId = playerId;
            
        }
        
        public void RecolorCityStructures()
        {
            if (CityStructure == null) return;
            GameObject city = SessionManager.Instance.Board.GetTileObject(CityStructure.Position.x, CityStructure.Position.y);
            city.GetComponent<TileGenerator>().RecolorHouses(CityStructure.OwnerId);
        }

        public string GetConfig()
        {
            return id + ":" + rotationIndex;
        }
        
        public string GetConfigWithoutRotation()
        {
            return id;
        }

        public void SetConfig(string config)
        {
            string[] parts = config.Split(':');
            if (parts.Length == 2)
            {
                id = parts[0];
                rotationIndex = int.Parse(parts[1]);
            }
        }

        public static string GetRotatedConfig(string config, int times = 1)
        {
            char[] sides = config.ToCharArray();
            char[] rotatedSides = new char[4];
            for (int i = 0; i < 4; i++)
            {
                int sourceIndex = (i - times + 4) % 4;
                rotatedSides[i] = sides[sourceIndex];
            }
            return new string(rotatedSides);
        }

        public static LandscapeType CharToLandscape(char c)
        {
            return c switch
            {
                'C' => LandscapeType.City,
                'R' => LandscapeType.Road,
                'F' => LandscapeType.Field,
                _ => throw new System.ArgumentException($"Invalid landscape type: {c}")
            };
        }

        public bool IsCity()
        {
            // check in Id if there is C
            return id.Contains('C');
        }

        public bool IsRoad()
        {
            return id.Contains('R');
        }
    }
}