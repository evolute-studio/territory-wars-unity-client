using UnityEngine;

namespace TerritoryWars
{
    public class TileView : MonoBehaviour
    {
        private static readonly Color CityColor = new Color(0.3f, 0.5f, 0.9f);    // Синій
        private static readonly Color RoadColor = new Color(0.5f, 0.35f, 0.2f);   // Коричневий
        private static readonly Color FieldColor = new Color(0.3f, 0.8f, 0.3f);   // Зелений

        private SpriteRenderer[] sideRenderers;
        private TileData tileData;
        private bool showDebugInfo = true;

        private void Awake()
        {
            // Отримуємо всі SpriteRenderer в правильному порядку
            Transform connectorsTransform = transform.Find("Connectors");
            sideRenderers = new SpriteRenderer[4]
            {
                connectorsTransform.Find("TopSide").GetComponent<SpriteRenderer>(),
                connectorsTransform.Find("RightSide").GetComponent<SpriteRenderer>(),
                connectorsTransform.Find("BottomSide").GetComponent<SpriteRenderer>(),
                connectorsTransform.Find("LeftSide").GetComponent<SpriteRenderer>()
            };
        }

        public void UpdateView(TileData newTileData)
        {
            if (newTileData == null) return;

            tileData = newTileData;
            for (int i = 0; i < 4; i++)
            {
                LandscapeType landscape = tileData.GetSide((Side)i);
                Color color = GetColorForLandscape(landscape);
                sideRenderers[i].color = color;
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo || tileData == null || !gameObject.activeSelf) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            if (screenPos.z <= 0) return; // Не показуємо, якщо тайл позаду камери

            // Конвертуємо координати екрану в GUI координати
            screenPos.y = Screen.height - screenPos.y;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontSize = 12
            };

            // Показуємо ID тайлу
            GUI.color = Color.black;
            GUI.Label(new Rect(screenPos.x - 49, screenPos.y - 9, 100, 20), tileData.id, style);
            GUI.color = Color.white;
            GUI.Label(new Rect(screenPos.x - 50, screenPos.y - 10, 100, 20), tileData.id, style);

            // Показуємо типи конекторів
            float offset = 30f; // Відступ від центру
            Vector2[] positions = new Vector2[]
            {
                new Vector2(screenPos.x, screenPos.y - offset),      // Top
                new Vector2(screenPos.x + offset, screenPos.y),      // Right
                new Vector2(screenPos.x, screenPos.y + offset),      // Bottom
                new Vector2(screenPos.x - offset, screenPos.y)       // Left
            };

            for (int i = 0; i < 4; i++)
            {
                LandscapeType type = tileData.GetSide((Side)i);
                string typeChar = type switch
                {
                    LandscapeType.City => "C",
                    LandscapeType.Road => "R",
                    LandscapeType.Field => "F",
                    _ => "?"
                };

                // Встановлюємо колір відповідно до типу
                GUI.color = type switch
                {
                    LandscapeType.City => CityColor,
                    LandscapeType.Road => RoadColor,
                    LandscapeType.Field => FieldColor,
                    _ => Color.white
                };

                // Додаємо чорну тінь для кращої видимості
                GUI.color = Color.black;
                GUI.Label(new Rect(positions[i].x - 9, positions[i].y - 9, 20, 20), typeChar, style);
                GUI.color = GetColorForLandscape(type);
                GUI.Label(new Rect(positions[i].x - 10, positions[i].y - 10, 20, 20), typeChar, style);
            }
            GUI.color = Color.white; // Скидаємо колір назад до білого
        }

        private Color GetColorForLandscape(LandscapeType type)
        {
            return type switch
            {
                LandscapeType.City => CityColor,
                LandscapeType.Road => RoadColor,
                LandscapeType.Field => FieldColor,
                _ => Color.white
            };
        }

        // Метод для вмикання/вимикання відображення debug інформації
        public void ToggleDebugInfo()
        {
            showDebugInfo = !showDebugInfo;
        }
    }
}