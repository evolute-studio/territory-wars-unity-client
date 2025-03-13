using System;
using System.Collections.Generic;
using TerritoryWars.UI;
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
        public List<HousesSprite> NeutralHousesAnimated;
        public Sprite[] SecondPlayerHouses;
        public Sprite[] Mountains;
        public GameObject ForestPrefab;

        public Sprite[] Pins; // 0 - neutral, 1 - first player, 2 - second player

        public int CurrentIndex { get; private set; } = 0;
        public int CurrentHouseIndex { get; private set; } = 0;

        public Sprite GetRandomHouse(int playerIndex)
        {
            Sprite[][] Houses = { FirstPlayerHouses, SecondPlayerHouses };
            int randomIndex = Random.Range(0, Houses[playerIndex].Length);
            Sprite randomHouse = Houses[playerIndex][randomIndex];
            return randomHouse;
        }

        public Sprite[] GetNextHouse(int playerIndex, bool chooseHighHouse = false)
        {
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
            if (playerIndex == -1)
            {
                Sprite[] neutralNextHouseSprites;
                if (chooseHighHouse)
                {
                    neutralNextHouseSprites = NeutralHousesAnimated[1].HousesSprites;
                    return neutralNextHouseSprites;
                }
                CurrentHouseIndex = (CurrentHouseIndex + 1) % NeutralHousesAnimated.Count;
                neutralNextHouseSprites = NeutralHousesAnimated[CurrentHouseIndex].HousesSprites;
                return neutralNextHouseSprites;
            }

            if (chooseHighHouse)
            {
                List<HousesSprite>[] highHouses = { FirstPlayerHousesAnimated, SecondPlayerHousesAnimated };
                Sprite[] highHouse = highHouses[playerIndex][1].HousesSprites;
                return highHouse;
            }
            
            List<HousesSprite>[] Houses = { FirstPlayerHousesAnimated, SecondPlayerHousesAnimated };
            CurrentHouseIndex = (CurrentHouseIndex + 1) % Houses[playerIndex].Count;
            Sprite[] nextHouseSprites = Houses[playerIndex][CurrentHouseIndex].HousesSprites;
            return nextHouseSprites;
        }
        
        public Sprite[] GetHouseByReference(Sprite[] sprites, int playerIndex)
        {
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
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

            int i = 0;
            foreach (var house in NeutralHousesAnimated)
            {
                if (house.HousesSprites == sprites)
                {
                    if (playerIndex == 0)
                    {
                        return FirstPlayerHousesAnimated[i].HousesSprites;
                    }

                    if (playerIndex == 1)
                    {
                        return SecondPlayerHousesAnimated[i].HousesSprites;
                    }

                    i++;
                }
            }

            return null;
        }
        
        public Sprite GetHouseByReference(Sprite sprites, int playerIndex){
            foreach (var house in FirstPlayerHouses)
            {
                if (house == sprites)
                {
                    if (playerIndex == 0)
                        return house;
                    else
                        return SecondPlayerHouses[Array.IndexOf(FirstPlayerHouses, house)];
                }
            }

            foreach (var house in SecondPlayerHouses)
            {
                if (house == sprites)
                {
                    if (playerIndex == 1)
                        return house;
                    else
                        return FirstPlayerHouses[Array.IndexOf(SecondPlayerHouses, house)];
                }
            }
            
            foreach (var house in NeutralHousesAnimated)
            {
                if (house.HousesSprites[0] == sprites)
                {
                    return house.HousesSprites[0];
                }
            }

            return null;
        }
        
        public Sprite GetPinByPlayerId(int playerId)
        {
            playerId = SetLocalPlayerData.GetLocalIndex(playerId);
            int id = playerId + 1;
            return Pins[id];
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