using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerritoryWars.ScriptablesObjects
{
    [CreateAssetMenu(fileName = "TileAssetsObject", menuName = "TileAssetsObject", order = 0)]
    public class TileAssetsObject : ScriptableObject
    {
        public Sprite[] FirstPlayerHouses;
        public List<HousesSprite> FirstPlayerHousesAnimated;
        public List<HousesSprite> SecondPlayerHousesAnimated;
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

        public Sprite[] GetNextHouse(int playerIndex)
        {
            List<HousesSprite>[] Houses = { FirstPlayerHousesAnimated, SecondPlayerHousesAnimated };
            CurrentHouseIndex = (CurrentHouseIndex + 1) % Houses[playerIndex].Count;
            Sprite[] nextHouseSprites = Houses[playerIndex][CurrentHouseIndex].HousesSprites;
            return nextHouseSprites;
        }
        
        public Sprite[] GetHouseByReference(Sprite[] sprites, int playerIndex)
        {
            foreach (var house in FirstPlayerHousesAnimated)
            {
                if (house.HousesSprites == sprites)
                {
                    if (playerIndex == 0)
                        return house.HousesSprites;
                    else
                        return SecondPlayerHousesAnimated[FirstPlayerHousesAnimated.IndexOf(house)].HousesSprites;
                }
            }

            foreach (var house in SecondPlayerHousesAnimated)
            {
                if (house.HousesSprites == sprites)
                {
                    if (playerIndex == 1)
                        return house.HousesSprites;
                    else
                        return FirstPlayerHousesAnimated[SecondPlayerHousesAnimated.IndexOf(house)].HousesSprites;
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
        
        [Serializable]
        public class HousesSprite
        {
            public Sprite[] HousesSprites;
        }
    }
}