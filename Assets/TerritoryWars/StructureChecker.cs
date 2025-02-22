using System;
using System.Collections.Generic;
using System.Linq;
using TerritoryWars;
using TerritoryWars.General;
using TerritoryWars.Tile;
using UnityEngine;

public class StructureChecker
{
    public Dictionary<Vector2Int, Structure> CityMap = new Dictionary<Vector2Int, Structure>();
    public Dictionary<Vector2Int, Structure> RoadMap = new Dictionary<Vector2Int, Structure>();
    public List<List<Structure>> CompletedStructures = new List<List<Structure>>();
    

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
            city = new Structure(null, new Vector2Int(x, y), cityCount, tileData);
            CityMap.Add(new Vector2Int(x, y), city);
            tileData.SetCityStructure(city);
        }
        if (roadCount > 0 && tileData.RoadStructure == null)
        {
            road = new Structure(null, new Vector2Int(x, y), roadCount, tileData);
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
            Contest(root1);
        }
    }
    
    public void Contest(Structure root)
    {
        root = FindRoot(root);
        int[] points = GetPoints(root);
        int winner = points[0] > points[1] ? 0 : 1;
        Debug.Log($"Player {winner} wins. Points: {points[0]} - {points[1]}");
        
        ChangeOwner(root, winner);
        
        List<Structure> completed = MoveToCompleted(root);

        GameObject contestAnimationGO = PrefabsManager.Instance.InstantiateObject(PrefabsManager.Instance.ClashAnimationPrefab);
        ClashAnimation contestAnimation = contestAnimationGO.GetComponent<ClashAnimation>();
        contestAnimation.Initialize(GetCompletedStructuresPosition(completed), winner, completed,(points[0] + points[1]) * 2);
    }
    
    private List<Structure> MoveToCompleted(Structure structure)
    {
        List<Structure> completedStructure = new List<Structure>();
        CollectStructureGroup(structure, completedStructure);
        CompletedStructures.Add(completedStructure);
        return completedStructure;
    }
    
    private void CollectStructureGroup(Structure structure, List<Structure> group)
    {
        group.Add(structure);
        
        if (structure.TileData.CityStructure == structure)
        {
            CityMap.Remove(structure.Position);
        }
        else if (structure.TileData.RoadStructure == structure)
        {
            RoadMap.Remove(structure.Position);
        }
        
        foreach (var child in structure.Children)
        {
            CollectStructureGroup(child, group);
        }
    }
    
    public void ChangeOwner(Structure structure, int newOwner)
    {
        structure.OwnerId = newOwner;
        structure.TileData.SetCityOwner(newOwner);
        foreach (var child in structure.Children)
        {
            ChangeOwner(child, newOwner);
        }
    }
    
    public int[] GetPoints(Structure root)
    {
        int[] points = new int[2];
        if (root.OwnerId >= 0)
        {
            points[root.OwnerId] = root.InitialOpenEdges;
        }
        foreach (var child in root.Children)
        {
            int[] childPoints = GetPoints(child);
            points[0] += childPoints[0];
            points[1] += childPoints[1];
        }
        return points;
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
        if (generalOpenEdges == 0)
        {
            return true;
        }
        return false;
    }

    private int GetOpenEdges(Structure structure)
    {
        int result = structure.OpenEdges;
        Structure current = structure;
        foreach (var child in current.Children)
        {
            result += GetOpenEdges(child);
        }
        return result;
    }
    
    public Vector3 GetCompletedStructuresPosition(List<Structure> structures)
    {
        Vector3 result = Vector3.zero;
        foreach (var structure in structures)
        {
            result += GameManager.Instance.Board.GetTilePosition(structure.Position.x, structure.Position.y);
        }
        result /= structures.Count;
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
    public int OwnerId = -1;
    public TileData TileData;
    public int InitialOpenEdges;
    
    public Structure(Structure root, Vector2Int position, int openEdges, TileData tileData)
    {
        Root = root;
        OpenEdges = openEdges;
        Position = position;
        TileData = tileData;
        OwnerId = tileData.OwnerId;
        InitialOpenEdges = openEdges;
    }
    

    public override string ToString()
    {
        return $"Structure at {Position}";
    }
}


