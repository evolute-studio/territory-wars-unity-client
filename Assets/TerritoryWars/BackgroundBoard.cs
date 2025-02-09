using UnityEngine;

namespace TerritoryWars
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
                    // Використовуємо ту саму формулу позиціонування, що й в основній дошці
                    float xPosition = (x - y) * 0.5f;
                    float yPosition = (x + y) * 0.25f;
                    Vector3 position = new Vector3(xPosition, yPosition, 0);

                    GameObject tile = Instantiate(backgroundTilePrefab, position, Quaternion.identity, transform);
                    tile.name = $"Tile_{x}_{y}";

                    // Додаємо колайдер, якщо його немає
                    if (!tile.GetComponent<BoxCollider2D>())
                    {
                        var collider = tile.AddComponent<BoxCollider2D>();
                        // Налаштовуємо розмір колайдера відповідно до спрайту
                        collider.size = new Vector2(1f, 0.5f); // Приблизні розміри для ізометричної проекції
                    }

                    // Встановлюємо правильний шар
                    tile.layer = LayerMask.NameToLayer("BackgroundBoard");
                }
            }
        }
    }
}