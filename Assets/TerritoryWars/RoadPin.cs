using TerritoryWars.UI;
using UnityEngine;

namespace TerritoryWars
{
    public class RoadPin : MonoBehaviour
    {
        public int PointsCount = 1;
        public SpriteRenderer SpriteRenderer;
        
        // 0 - neutral, 1 - first player, 2 - second player
        // 3 - neutral two points, 4 - first player two points, 5 - second player two points
        public Sprite[] PinsSprites;
        
        public void Initialize(int playerIndex, int pointsCount)
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            PointsCount = pointsCount;
            SetPin(playerIndex);
        }
        
        public void SetPin(int playerIndex)
        {
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex) + 1;
            int pinIndex = playerIndex + (PointsCount - 1) * 3;
            SpriteRenderer.sprite = PinsSprites[pinIndex];
        }


    }
}