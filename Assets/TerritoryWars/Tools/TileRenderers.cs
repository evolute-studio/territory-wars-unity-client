using System;
using System.Collections.Generic;
using TerritoryWars.Tile;
using UnityEngine;

public class TileRenderers : MonoBehaviour
{
    public List<SpriteRenderer> HouseRenderers;
    public List<SpriteRenderer> ArcRenderers;
    public TerritoryFiller TileTerritoryFiller;
    public FencePlacer TileFencePlacer;
    public SpriteRenderer RoadRenderers;
    public GameObject Mill;
    public List<CloserToBorderFence> CloserToBorderFences;

    [Serializable]
    public class CloserToBorderFence
    {
        public Side Side;
        public GameObject Fence;
    }
}
