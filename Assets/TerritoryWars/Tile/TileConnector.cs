using UnityEngine;

namespace TerritoryWars.Tile
{
    public class TileConnector : MonoBehaviour
    {
        public LandscapeType landscapeType;
        
        public void SetLandscape(LandscapeType landscape)
        {
            landscapeType = landscape;
            GetComponent<SpriteRenderer>().color = GetColorForLandscape(landscape);
        }
        
        private Color GetColorForLandscape(LandscapeType landscape)
        {
            switch (landscape)
            {
                case LandscapeType.City:
                    return new Color(0.3f, 0.5f, 0.9f); 
                case LandscapeType.Road:
                    return new Color(0.5f, 0.35f, 0.2f);
                case LandscapeType.Field:
                    return new Color(0.3f, 0.8f, 0.3f);
                default:
                    return Color.white;
            }
        }
    }
}