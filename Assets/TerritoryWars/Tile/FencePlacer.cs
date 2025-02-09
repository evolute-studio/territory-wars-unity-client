using System.Collections;
using UnityEngine;

namespace TerritoryWars.Tile
{
    [RequireComponent(typeof(LineRenderer))]
    public class FencePlacer : MonoBehaviour
    {
        [Header("Fence Sprites")]
        [SerializeField] private Sprite fenceTop;        // ─
        [SerializeField] private Sprite fenceTopRight;   // ╲
        [SerializeField] private Sprite fenceRight;      // │
        [SerializeField] private Sprite fenceBottomRight;// ╱
        [SerializeField] private Sprite fenceBottom;     // ─
        [SerializeField] private Sprite fenceBottomLeft; // ╲
        [SerializeField] private Sprite fenceLeft;       // │
        [SerializeField] private Sprite fenceTopLeft;    // ╱

        [Header("Settings")]
        [SerializeField] private GameObject fencePrefab;
        [SerializeField] private float spacing = 0.5f;
        [SerializeField] private float fenceWidth = 1f; // Ширина одного забору

        [Header("Animation")]
        [SerializeField] private float spawnDelay = 0.5f; // Затримка між спавном заборів
        [SerializeField] private bool showDebugInfo = false; // Додаємо перемикач для відображення debug інформації

        [Header("Territory")]
        [SerializeField] private GameObject territoryPrefab;
        private Territory currentTerritory;

        public LineRenderer lineRenderer;

        [ContextMenu("Place Fence")]
        public void PlaceFence()
        {
            if (fencePrefab == null || lineRenderer == null || lineRenderer.positionCount < 2)
            {
                Debug.LogError("Не задано префаб або потрібно хоча б дві точки у LineRenderer.");
                return;
            }

            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            StartCoroutine(PlaceFenceRoutine());
        }

        private IEnumerator PlaceFenceRoutine()
        {
            Vector3[] points = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(points);

            Debug.Log($"Got {points.Length} points from LineRenderer");

            // Конвертуємо в локальні координати
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = transform.InverseTransformPoint(points[i]);
                Debug.Log($"Point {i}: {points[i]}");
            }

            // Створюємо нову територію
            if (currentTerritory != null)
            {
                Debug.Log("Destroying old territory");
                DestroyImmediate(currentTerritory.gameObject);
            }

            if (territoryPrefab == null)
            {
                Debug.LogError("Territory prefab is not assigned!");
                yield break;
            }

            Debug.Log("Creating new territory");
            currentTerritory = Instantiate(territoryPrefab, transform).GetComponent<Territory>();
            currentTerritory.SetLineRenderer(lineRenderer);
            currentTerritory.GenerateMask();

            // Проходимо всі точки по порядку
            for (int i = 0; i < points.Length - 1; i++)
            {
                Debug.Log($"Processing segment {i} -> {i + 1}");
                yield return StartCoroutine(PlaceFenceSegmentRoutine(points[i], points[i + 1]));
            }
        }

        private IEnumerator PlaceFenceSegmentRoutine(Vector3 start, Vector3 end)
        {
            Vector2 direction = (end - start).normalized;

            if (float.IsNaN(direction.x) || float.IsNaN(direction.y))
            {
                Debug.LogError($"Invalid direction: {direction}, start: {start}, end: {end}");
                yield break;
            }

            Debug.Log($"Placing fence from {start} to {end}, direction: {direction}");
            int spriteIndex = GetIsometricDirectionIndex(direction);
            float distance = Vector2.Distance(start, end);

            int fenceCount = Mathf.FloorToInt(distance / fenceWidth);
            if (fenceCount < 1) fenceCount = 1;

            float actualSpacing = distance / fenceCount;

            for (int i = 0; i < fenceCount; i++)
            {
                float t = (i + 0.5f) / fenceCount;
                Vector3 localPosition = Vector3.Lerp(start, end, t);

                GameObject fence = Instantiate(fencePrefab, transform);
                fence.transform.localPosition = localPosition;
                fence.transform.position += transform.position;

                var spriteRenderer = fence.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = GetFenceSprite(spriteIndex);
                }

                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private int GetIsometricDirectionIndex(Vector2 direction)
        {
            // Конвертуємо напрямок в ізометричні координати


            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

            // Розділяємо на 8 секторів (по 45 градусів)
            int index = Mathf.RoundToInt(angle / 45f) % 8;

            return index;
        }

        private Sprite GetFenceSprite(int directionIndex)
        {
            return directionIndex switch
            {
                0 => fenceTop,
                1 => fenceTopRight,
                2 => fenceRight,
                3 => fenceBottomRight,
                4 => fenceBottom,
                5 => fenceBottomLeft,
                6 => fenceLeft,
                7 => fenceTopLeft,
                _ => fenceTop // За замовчуванням
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (lineRenderer == null || lineRenderer.positionCount < 2) return;

            Vector3[] points = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(points);

            for (int i = 0; i < points.Length; i++)
            {
                points[i] += transform.position;
            }

            Gizmos.color = Color.yellow;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 worldStart = points[i];
                Vector3 worldEnd = points[i + 1];

                // Малюємо лінію між точками
                Gizmos.DrawLine(worldStart, worldEnd);

                Vector3 localStart = transform.InverseTransformPoint(worldStart);
                Vector3 localEnd = transform.InverseTransformPoint(worldEnd);
                Vector2 direction = (localEnd - localStart).normalized;

                if (!float.IsNaN(direction.x) && !float.IsNaN(direction.y))
                {
                    int spriteIndex = GetIsometricDirectionIndex(direction);
                    float distance = Vector2.Distance(localStart, localEnd);
                    int fenceCount = Mathf.Max(1, Mathf.FloorToInt(distance / fenceWidth));

                    Gizmos.color = Color.green;
                    for (int j = 0; j < fenceCount; j++)
                    {
                        float t = (j + 0.5f) / fenceCount;
                        Vector3 spawnPoint = Vector3.Lerp(worldStart, worldEnd, t);
                        Gizmos.DrawWireSphere(spawnPoint, 0.01f);

                        if (showDebugInfo) // Показуємо текст тільки якщо включено debug інформацію
                        {
                            // Формуємо текст для відображення
                            string info = $"Spawn Point {j}:\n" +
                                        $"Start: P{i}({worldStart.x:F2}, {worldStart.y:F2})\n" +
                                        $"End: P{i + 1}({worldEnd.x:F2}, {worldEnd.y:F2})\n" +
                                        $"Angle: {GetAngleFromDirection(direction):F1}°";

                            // Зміщуємо текст трохи вправо і вгору від точки
                            Vector3 labelPos = spawnPoint + new Vector3(0.05f, 0.05f, 0);
                            UnityEditor.Handles.Label(labelPos, info);
                        }
                    }
                    Gizmos.color = Color.yellow;
                }
            }
        }

        private float GetAngleFromDirection(Vector2 direction)
        {
            Vector2 isoDirection = new Vector2(
                direction.x - direction.y,
                (direction.x + direction.y)
            ).normalized;

            float angle = Mathf.Atan2(isoDirection.y, isoDirection.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            return angle;
        }
    }
}