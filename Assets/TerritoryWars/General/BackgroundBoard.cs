using UnityEngine;

namespace TerritoryWars.General
{
    public class BackgroundBoard : MonoBehaviour
    {
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private GameObject backgroundTilePrefab;

        private void Awake()
        {
            CreateBoard();
        }

        private void CreateBoard()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    
                    float xPosition = (x - y) * 0.5f;
                    float yPosition = (x + y) * 0.25f;
                    Vector3 position = new Vector3(xPosition, yPosition, 0);

                    GameObject tile = Instantiate(backgroundTilePrefab, position, Quaternion.identity, transform);
                    tile.name = $"Tile_{x}_{y}";

                    
                    if (!tile.GetComponent<BoxCollider2D>())
                    {
                        var collider = tile.AddComponent<BoxCollider2D>();
                        
                        collider.size = new Vector2(1f, 0.5f);
                    }

                    
                    tile.layer = LayerMask.NameToLayer("BackgroundBoard");
                }
            }
        }
    }
}