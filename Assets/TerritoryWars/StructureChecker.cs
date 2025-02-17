using System;
using System.Collections.Generic;
using System.Linq;
using TerritoryWars.General;
using TerritoryWars.Tile;
using UnityEngine;

public class StructureChecker
{
    public Dictionary<Vector2Int, Structure> CityMap = new Dictionary<Vector2Int, Structure>();
    public Dictionary<Vector2Int, Structure> RoadMap = new Dictionary<Vector2Int, Structure>();
    

    public StructureChecker(Board board)
    {
        board.OnTilePlaced += CreateStructures;
    }
    

    public void CreateStructures(TileData tileData, int x, int y)
    {
        string config = tileData.id;
        int cityCount = config.Count(c => c == 'C');
        int roadCount = config.Count(c => c == 'R');
        Structure city = null;
        Structure road = null;
        if (cityCount > 0 && tileData.CityStructure == null)
        {
            city = new Structure(null, new Vector2Int(x, y), cityCount);
            CityMap.Add(new Vector2Int(x, y), city);
            tileData.SetCityStructure(city);
        }
        if (roadCount > 0 && tileData.RoadStructure == null)
        {
            road = new Structure(null, new Vector2Int(x, y), roadCount);
            RoadMap.Add(new Vector2Int(x, y), road);
            tileData.SetRoadStructure(road);
        }
    }
    
    public void UnionStructures(Structure city1, Structure city2)
    {
        Structure root1 = FindRoot(city1);
        Structure root2 = FindRoot(city2);
        if (root1 != root2)
        {
            root1.Root = root2;
            city2.Children.Add(city1);
            
        }
        city1.OpenEdges--;
        city2.OpenEdges--;
        if (city1.OpenEdges < 0) city1.OpenEdges = 0;
        if (city2.OpenEdges < 0) city2.OpenEdges = 0;
        
        Debug.Log("UnionStructures. Structure1: " + city1.Position + " Structure2: " + city2.Position);
        
        if (CheckCityCompletion(root1))
        {
            Debug.Log("City completed: " + root1.Position);
        }
    }
    
    public Structure FindRoot(Structure structure)
    {
        if (structure.Root == null)
        {
            return structure;
        }
        return FindRoot(structure.Root);
    }
    

    public bool CheckCityCompletion(Structure structure)
    {
        Structure root = FindRoot(structure);
        int generalOpenEdges = GetOpenEdges(root);
        Debug.Log("CheckCityCompletion. Structure: " + structure.Position + " GeneralOpenEdges: " + generalOpenEdges);
        if (generalOpenEdges == 0)
        {
            return true;
        }
        return false;
    }

    private int GetOpenEdges(Structure structure)
    {
        Debug.Log("GetOpenEdges. Structure: " + structure.Position + " OpenEdges: " + structure.OpenEdges);
        int result = structure.OpenEdges;
        Structure current = structure;
        foreach (var child in current.Children)
        {
            result += GetOpenEdges(child);
        }
        return result;
    }
    
}

[Serializable]
public class Structure
{
    public Structure Root;
    public List<Structure> Children = new List<Structure>();
    public Vector2Int Position;
    public int OpenEdges;
    
    public Structure(Structure root, Vector2Int position, int openEdges)
    {
        Root = root;
        OpenEdges = openEdges;
        Position = position;
    }

    public override string ToString()
    {
        return $"Structure at {Position}";
    }
}


