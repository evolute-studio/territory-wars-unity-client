using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.Tile
{
    public class TileClickHandler : MonoBehaviour
    {
        private TileSelector selector;
        private int x, y;

        public void Initialize(TileSelector selector, int xPos, int yPos)
        {
            this.selector = selector;
            x = xPos;
            y = yPos;
        }

        private void OnMouseDown()
        {
            selector.OnTileClicked(x, y);
        }
    }
}