using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerritoryWars.ScriptablesObjects
{
    [CreateAssetMenu(fileName = "TileAssetsObject", menuName = "TileAssetsObject", order = 0)]
    public class TileAssetsObject : ScriptableObject
    {
        public Sprite[] FirstPlayerHouses;
        public Sprite[] SecondPlayerHouses;
        public Sprite[] Mountains;
        public GameObject ForestPrefab;

        public int CurrentIndex { get; private set; } = 0;
        public int CurrentHouseIndex { get; private set; } = 0;

        public Sprite GetRandomHouse(int playerIndex)
        {
            Sprite[][] Houses = { FirstPlayerHouses, SecondPlayerHouses };
            int randomIndex = Random.Range(0, Houses[playerIndex].Length);
            Sprite randomHouse = Houses[playerIndex][randomIndex];
            return randomHouse;
        }

        public Sprite GetNextHouse(int playerIndex)
        {
            Sprite[][] Houses = { FirstPlayerHouses, SecondPlayerHouses };
            CurrentHouseIndex = (CurrentHouseIndex + 1) % Houses[playerIndex].Length;
            Sprite nextHouse = Houses[playerIndex][CurrentHouseIndex];
            return nextHouse;
        }
        
        public Sprite GetHouseByReference(Sprite sprite, int playerIndex)
        {
            foreach (var house in FirstPlayerHouses)
            {
                if (house == sprite)
                {
                    if (playerIndex == 0)
                        return house;
                    else
                        return SecondPlayerHouses[Array.IndexOf(FirstPlayerHouses, house)];
                }
            }
            
            foreach (var house in SecondPlayerHouses)
            {
                if (house == sprite)
                {
                    if (playerIndex == 1)
                        return house;
                    else
                        return FirstPlayerHouses[Array.IndexOf(SecondPlayerHouses, house)];
                }
            }
            
            return null;
        }

        public void BackIndex(int times)
        {
            CurrentHouseIndex = (CurrentHouseIndex - times) % FirstPlayerHouses.Length;
            if (CurrentHouseIndex < 0)
                CurrentHouseIndex += FirstPlayerHouses.Length;
        }

        public Sprite GetRandomMountain()
        {
            int randomIndex = Random.Range(0, Mountains.Length);
            Sprite randomMountain = Mountains[randomIndex];
            return randomMountain;
        }

    }
}