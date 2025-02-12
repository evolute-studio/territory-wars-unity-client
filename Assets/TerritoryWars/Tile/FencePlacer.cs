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
    }
}