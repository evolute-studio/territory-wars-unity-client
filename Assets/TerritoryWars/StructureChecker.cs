using System.Collections.Generic;
using TerritoryWars.Tile;
using UnityEngine;

public class StructureChecker : MonoBehaviour
{
    // Клас для представлення набору міст
    private class CitySet
    {
        public HashSet<Vector2Int> positions = new HashSet<Vector2Int>(); // Позиції тайлів, що містять це місто
        public int openEdges = 0; // Кількість відкритих країв міста
    }

    private Dictionary<Vector2Int, CitySet> citySetsByPosition = new Dictionary<Vector2Int, CitySet>();
    private List<CitySet> citySets = new List<CitySet>();
    // list for completed cities
    private List<CitySet> completedCities = new List<CitySet>();
    

    public void CheckAndUnionCities(Vector2Int position, TileData tileData, Dictionary<Vector2Int, TileData> placedTiles)
    {
        CitySet newCitySet = new CitySet();
        newCitySet.positions.Add(position);

        // Підрахунок відкритих країв для нового тайлу
        int cityEdgesCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (tileData.GetSide((Side)i) == LandscapeType.City)
            {
                cityEdgesCount++;
            }
        }
        newCitySet.openEdges = cityEdgesCount;

        // Перевірка сусідніх тайлів
        Vector2Int[] adjacentPositions = new Vector2Int[]
        {
            new Vector2Int(position.x, position.y + 1), // Top
            new Vector2Int(position.x + 1, position.y), // Right
            new Vector2Int(position.x, position.y - 1), // Bottom
            new Vector2Int(position.x - 1, position.y)  // Left
        };

        HashSet<CitySet> connectedSets = new HashSet<CitySet>();

        for (int i = 0; i < 4; i++)
        {
            if (tileData.GetSide((Side)i) != LandscapeType.City)
                continue;

            Vector2Int adjPos = adjacentPositions[i];
            if (!placedTiles.TryGetValue(adjPos, out TileData adjTile))
                continue;

            // Перевіряємо, чи має сусідній тайл місто на протилежній стороні
            Side oppositeSize = (Side)(((int)i + 2) % 4);
            if (adjTile.GetSide(oppositeSize) == LandscapeType.City)
            {
                if (citySetsByPosition.TryGetValue(adjPos, out CitySet adjacentSet))
                {
                    connectedSets.Add(adjacentSet);
                    newCitySet.openEdges--; // Зменшуємо кількість відкритих країв
                }
            }
        }

        // Об'єднуємо всі з'єднані набори
        if (connectedSets.Count > 0)
        {
            CitySet mergedSet = new CitySet();
            foreach (var set in connectedSets)
            {
                mergedSet.openEdges += set.openEdges;
                foreach (var pos in set.positions)
                {
                    mergedSet.positions.Add(pos);
                    citySetsByPosition[pos] = mergedSet;
                }
                citySets.Remove(set);
            }
            
            mergedSet.positions.Add(position);
            mergedSet.openEdges += newCitySet.openEdges;
            citySetsByPosition[position] = mergedSet;
            citySets.Add(mergedSet);
        }
        else
        {
            // Якщо немає з'єднань, створюємо новий набір
            citySets.Add(newCitySet);
            citySetsByPosition[position] = newCitySet;
        }

        // Перевіряємо завершеність міста
        CheckCityCompletion(citySetsByPosition[position]);
    }

    private void CheckCityCompletion(CitySet citySet)
    {
        if (citySet.openEdges == 0)
        {
            Debug.Log($"Місто завершено! Розмір: {citySet.positions.Count} тайлів");
            // Тут можна додати логіку підрахунку очок
        }
    }
}


