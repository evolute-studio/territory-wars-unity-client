using UnityEngine;

namespace TerritoryWars.Tile
{
    public class TileClickHandler : MonoBehaviour
    {
        private General.GameManager gameManager;
        private int x, y;

        public void Initialize(General.GameManager manager, int xPos, int yPos)
        {
            gameManager = manager;
            x = xPos;
            y = yPos;
        }

        private void OnMouseDown()
        {
            gameManager.OnTileClicked(x, y);
        }
    }
}