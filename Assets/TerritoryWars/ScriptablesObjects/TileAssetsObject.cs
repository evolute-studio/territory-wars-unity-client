using UnityEngine;

namespace TerritoryWars.ScriptablesObjects
{
    [CreateAssetMenu(fileName = "TileAssetsObject", menuName = "TileAssetsObject", order = 0)]
    public class TileAssetsObject : ScriptableObject
    {
        public Sprite[] Houses;
        public Sprite[] Mountains;
        
        private int currentIndex = 0;
        
        public Sprite GetRandomHouse()
        {
            int randomIndex = Random.Range(0, Houses.Length);
            Sprite randomHouse = Houses[randomIndex];
            return randomHouse;
        }
        
        public Sprite GetNextHouse()
        {
            currentIndex = (currentIndex + 1) % Houses.Length;
            Sprite nextHouse = Houses[currentIndex];
            return nextHouse;
        }
        
        public Sprite GetRandomMountain()
        {
            int randomIndex = Random.Range(0, Mountains.Length);
            Sprite randomMountain = Mountains[randomIndex];
            return randomMountain;
        }
        
    }
}