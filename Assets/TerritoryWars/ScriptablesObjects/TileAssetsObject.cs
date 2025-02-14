using UnityEngine;

namespace TerritoryWars.ScriptablesObjects
{
    [CreateAssetMenu(fileName = "TileAssetsObject", menuName = "TileAssetsObject", order = 0)]
    public class TileAssetsObject : ScriptableObject
    {
        public Sprite[] FirstPlayerHouses;
        public Sprite[] SecondPlayerHouses;
        public Sprite[] Mountains;
        public GameObject ForestPrefab;
        
        private int currentIndex = 0;
        
        public Sprite GetRandomHouse(int playerIndex)
        {
            Sprite[][] Houses = {FirstPlayerHouses, SecondPlayerHouses};
            int randomIndex = Random.Range(0, Houses[playerIndex].Length);
            Sprite randomHouse = Houses[playerIndex][randomIndex];
            return randomHouse;
        }
        
        public Sprite GetNextHouse(int playerIndex)
        {
            Sprite[][] Houses = {FirstPlayerHouses, SecondPlayerHouses};
            currentIndex = (currentIndex + 1) % Houses.Length;
            Sprite nextHouse = Houses[playerIndex][currentIndex];
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