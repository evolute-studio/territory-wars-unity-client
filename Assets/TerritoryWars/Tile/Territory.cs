using System.Collections.Generic;
using UnityEngine;

namespace TerritoryWars.Tile
{
    public class Territory : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private SpriteMask spriteMask;
        [SerializeField] private SpriteRenderer fillTexture;

        [Header("Settings")]
        [SerializeField] private int textureSize = 512;
        [SerializeField] private float lineWidth = 2f;
        [SerializeField] private Color fillColor = Color.white;

        public void SetLineRenderer(LineRenderer lineRenderer)
        {
            this.lineRenderer = lineRenderer;
        }
        
       

        public void GenerateMask()
        {
            if (lineRenderer == null || lineRenderer.positionCount < 3)
            {
                Debug.LogError("LineRenderer is missing or has less than 3 points");
                return;
            }

            int pointCount = lineRenderer.positionCount;
            
            Vector3[] worldPoints = new Vector3[pointCount];
            lineRenderer.GetPositions(worldPoints);

            
            Vector3[] localPoints = new Vector3[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
                localPoints[i] = transform.InverseTransformPoint(worldPoints[i] + transform.position);
            }

            
            Texture2D texture = new Texture2D(textureSize, textureSize);
            texture.filterMode = FilterMode.Point;

           
            Color[] clearColors = new Color[textureSize * textureSize];
            for (int i = 0; i < clearColors.Length; i++)
            {
                clearColors[i] = Color.clear;
            }
            texture.SetPixels(clearColors);

           
            Bounds bounds = GetBounds(localPoints);
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y);
            Vector2 center = bounds.center;

            
            Vector2[] texturePoints = new Vector2[localPoints.Length];
            for (int i = 0; i < localPoints.Length; i++)
            {
                
                texturePoints[i] = new Vector2(
                    (localPoints[i].x - center.x) * (textureSize / maxSize) + (textureSize / 2),
                    (localPoints[i].y - center.y) * (textureSize / maxSize) + (textureSize / 2)
                );
            }

            
            for (int i = 0; i < texturePoints.Length; i++)
            {
                Vector2 start = texturePoints[i];
                Vector2 end = texturePoints[(i + 1) % texturePoints.Length];
                DrawLine(texture, start, end, Color.white);
            }

            
            FloodFill(texture, textureSize / 2, textureSize / 2, Color.white);

            texture.Apply();

            
            Sprite maskSprite = Sprite.Create(
                texture,
                new Rect(0, 0, textureSize, textureSize),
                Vector2.one * 0.5f,
                100f
            );

            
            spriteMask.sprite = maskSprite;
            spriteMask.transform.localPosition = bounds.center;
            spriteMask.transform.localScale = new Vector3(maxSize, maxSize, 1);

            
            if (fillTexture != null)
            {
                fillTexture.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                fillTexture.transform.localPosition = bounds.center;
                fillTexture.transform.localScale = new Vector3(maxSize, maxSize, 1);
                fillTexture.color = fillColor;
            }
        }

        private void DrawLine(Texture2D texture, Vector2 start, Vector2 end, Color color)
        {
            int x0 = Mathf.RoundToInt(start.x);
            int y0 = Mathf.RoundToInt(start.y);
            int x1 = Mathf.RoundToInt(end.x);
            int y1 = Mathf.RoundToInt(end.y);

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                
                for (int w = -Mathf.FloorToInt(lineWidth / 2); w <= lineWidth / 2; w++)
                {
                    for (int h = -Mathf.FloorToInt(lineWidth / 2); h <= lineWidth / 2; h++)
                    {
                        int px = x0 + w;
                        int py = y0 + h;
                        if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                        {
                            texture.SetPixel(px, py, color);
                        }
                    }
                }

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private void FloodFill(Texture2D texture, int x, int y, Color fillColor)
        {
            Color targetColor = texture.GetPixel(x, y);
            if (targetColor == fillColor) return;

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(x, y));

            while (queue.Count > 0)
            {
                Vector2Int pos = queue.Dequeue();
                if (pos.x < 0 || pos.x >= texture.width || pos.y < 0 || pos.y >= texture.height)
                    continue;

                Color pixelColor = texture.GetPixel(pos.x, pos.y);
                if (pixelColor != targetColor || pixelColor == fillColor)
                    continue;

                texture.SetPixel(pos.x, pos.y, fillColor);

                queue.Enqueue(new Vector2Int(pos.x + 1, pos.y));
                queue.Enqueue(new Vector2Int(pos.x - 1, pos.y));
                queue.Enqueue(new Vector2Int(pos.x, pos.y + 1));
                queue.Enqueue(new Vector2Int(pos.x, pos.y - 1));
            }
        }

        private Bounds GetBounds(Vector3[] points)
        {
            if (points == null || points.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            Bounds bounds = new Bounds(points[0], Vector3.zero);
            for (int i = 1; i < points.Length; i++)
            {
                bounds.Encapsulate(points[i]);
            }
            return bounds;
        }
    }
}