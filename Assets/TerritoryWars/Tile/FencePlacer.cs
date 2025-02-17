using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TerritoryWars.Tile
{
    public class FencePlacer : MonoBehaviour
    {
        public List<Transform> pillars;
        public float fenceYOffset;
        public LineRenderer lineRenderer;
        public int[] skipIndices;

        [ContextMenu("PlaceFence")]
        public void PlaceFence()
        {
            if (pillars == null || pillars.Count < 2) return;

            List<Vector3> points = new List<Vector3>();

            for (int i = 0; i < pillars.Count; i++)
            {
                Vector3 pillarPos = pillars[i].localPosition;
                Vector3 fencePoint = pillarPos + Vector3.up * fenceYOffset;

                // Якщо поточний індекс треба пропустити
                if (skipIndices != null && skipIndices.Contains(i))
                {
                    // Додаємо дві точки для створення розриву
                    if (i < pillars.Count - 1)
                    {
                        Vector3 nextPillarPos = pillars[i + 1].localPosition;
                        Vector3 nextFencePoint = nextPillarPos + Vector3.up * fenceYOffset;

                        // Додаємо точку перед розривом
                        points.Add(fencePoint);
                        // Опускаємо лінію вниз
                        points.Add(new Vector3(fencePoint.x - 0.001f, fencePoint.y, -150f));
                        // Піднімаємо лінію в наступній точці
                        points.Add(new Vector3(nextFencePoint.x + 0.001f, nextFencePoint.y, -150f));
                        points.Add(nextFencePoint);

                        // Пропускаємо наступну точку, оскільки ми вже її додали
                        i++;
                    }
                }
                else
                {
                    points.Add(fencePoint);
                }
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }


        // Альтернативний підхід через використання двох окремих сегментів
        [ContextMenu("PlaceFenceAlternative")]
        private void PlaceFenceAlternative()
        {
            if (pillars == null || pillars.Count < 2) return;

            List<Vector3> points = new List<Vector3>();

            for (int i = 0; i < pillars.Count - 1; i++)
            {
                Vector3 currentPos = pillars[i].localPosition + Vector3.up * fenceYOffset;
                Vector3 nextPos = pillars[i + 1].localPosition + Vector3.up * fenceYOffset;

                if (skipIndices != null && skipIndices.Contains(i))
                {
                    // Створюємо два окремі сегменти замість одного
                    float gapSize = 0.5f; // Розмір пробілу
                    Vector3 direction = (nextPos - currentPos).normalized;

                    // Додаємо точку поточного стовпа
                    points.Add(currentPos);
                    // Додаємо точку трохи не доходячи до наступного стовпа
                    points.Add(nextPos - direction * gapSize);
                    // Починаємо новий сегмент
                    points.Add(nextPos - direction * gapSize);
                    points.Add(nextPos);
                }
                else
                {
                    points.Add(currentPos);
                    points.Add(nextPos);
                }
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        [ContextMenu("Place Poles")]
        private void PlacePoles()
        {
            if (pillars == null || pillars.Count < 3) return;

            // Розраховуємо довжину лінії LineRenderer
            float lineLength = 0f;
            Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(linePoints);
            for (int i = 0; i < linePoints.Length - 1; i++)
            {
                lineLength += Vector3.Distance(linePoints[i], linePoints[i + 1]);
            }

            // Розраховуємо кількість проміжків між проміжними стовпчиками
            int gapCount = pillars.Count - 2;

            // Розраховуємо довжину одного проміжку
            float gapLength = lineLength / (gapCount + 1);

            // Розміщуємо проміжні стовпчики на рівній відстані вздовж лінії
            float currentLength = gapLength;
            int lineIndex = 0;
            for (int i = 1; i < pillars.Count - 1; i++)
            {
                while (lineIndex < linePoints.Length - 1 && currentLength >= Vector3.Distance(linePoints[lineIndex], linePoints[lineIndex + 1]))
                {
                    currentLength -= Vector3.Distance(linePoints[lineIndex], linePoints[lineIndex + 1]);
                    lineIndex++;
                }

                if (lineIndex >= linePoints.Length - 1)
                {
                    pillars[i].position = linePoints[linePoints.Length - 1];
                }
                else
                {
                    Vector3 direction = (linePoints[lineIndex + 1] - linePoints[lineIndex]).normalized;
                    Vector3 polePosition = linePoints[lineIndex] + direction * currentLength;

                    // Враховуємо якір знизу стовпчика
                    float poleHeight = pillars[i].localScale.y;
                    polePosition.y -= poleHeight / 2f;

                    // Переміщуємо стовпчик на відстань 0.05 вгору по осі Y
                    polePosition += Vector3.up * 0.05f;

                    pillars[i].position = polePosition;
                }

                currentLength += gapLength;
            }
        }
    }
}