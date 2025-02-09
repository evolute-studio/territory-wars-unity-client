using UnityEngine;

namespace TerritoryWars.ScriptablesObjects
{
    [CreateAssetMenu(fileName = "TileAssetsObject", menuName = "TileAssetsObject", order = 0)]
    public class TileAssetsObject : ScriptableObject
    {
        public Sprite[] Houses;
        
        public Sprite GetRandomHouse()
        {
            int randomIndex = Random.Range(0, Houses.Length);
            Sprite randomHouse = Houses[randomIndex];
            return randomHouse;
        }
        
    }
}