using UnityEngine;

namespace TerritoryWars
{
    public class TileClickHandler : MonoBehaviour
    {
        private GameManager gameManager;
        private int x, y;

        public void Initialize(GameManager manager, int xPos, int yPos)
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