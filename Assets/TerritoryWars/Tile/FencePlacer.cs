using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TerritoryWars.Tile
{
    public class FencePlacer : MonoBehaviour
    {
        public List<Transform> pillars;
        public float fenceYOffset = 0.08f;
        public LineRenderer lineRenderer;
        public int[] skipIndices;
        public float poleYOffset = 0f;

        [ContextMenu("PlaceFence")]
        public void PlaceFence()
        {
            if (pillars == null || pillars.Count < 2) return;

            List<Vector3> points = new List<Vector3>();

            for (int i = 0; i < pillars.Count; i++)
            {
                Vector3 pillarPos = pillars[i].localPosition;
                Vector3 fencePoint = pillarPos + Vector3.up * fenceYOffset;

                
                if (skipIndices != null && skipIndices.Contains(i))
                {
                    
                    if (i < pillars.Count - 1)
                    {
                        Vector3 nextPillarPos = pillars[i + 1].localPosition;
                        Vector3 nextFencePoint = nextPillarPos + Vector3.up * fenceYOffset;

                        
                        points.Add(fencePoint);
                        
                        points.Add(new Vector3(fencePoint.x - 0.001f, fencePoint.y, -150f));
                        
                        points.Add(new Vector3(nextFencePoint.x + 0.001f, nextFencePoint.y, -150f));
                        points.Add(nextFencePoint);

                        
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
                    
                    float gapSize = 0.5f; 
                    Vector3 direction = (nextPos - currentPos).normalized;

                    
                    points.Add(currentPos);
                    
                    points.Add(nextPos - direction * gapSize);
                    
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

            
            Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(linePoints);

            
            List<int> breakIndices = new List<int>();
            for (int i = 1; i < linePoints.Length - 1; i++)
            {
                if (linePoints[i].z < -100f)
                {
                    breakIndices.Add(i);
                }
            }

            
            List<Vector3[]> segments = new List<Vector3[]>();
            int startIndex = 0;
            foreach (int breakIndex in breakIndices)
            {
                int segmentLength = breakIndex - startIndex + 1;
                Vector3[] segment = new Vector3[segmentLength];
                System.Array.Copy(linePoints, startIndex, segment, 0, segmentLength);
                segments.Add(segment);
                startIndex = breakIndex + 1;
            }
            
            int lastSegmentLength = linePoints.Length - startIndex;
            Vector3[] lastSegment = new Vector3[lastSegmentLength];
            System.Array.Copy(linePoints, startIndex, lastSegment, 0, lastSegmentLength);
            segments.Add(lastSegment);

            
            int pillarIndex = 1;
            foreach (Vector3[] segment in segments)
            {
                float segmentLength = CalculateSegmentLength(segment);
                int segmentPoleCount = CountPolesInSegment(segment, pillarIndex);
                float gapLength = segmentLength / (segmentPoleCount + 1);

                float currentLength = gapLength;
                int segmentIndex = 0;
                while (pillarIndex < pillars.Count - 1 && segmentIndex < segment.Length - 1)
                {
                    Vector3 point1 = segment[segmentIndex];
                    Vector3 point2 = segment[segmentIndex + 1];
                    float dx = point2.x - point1.x;
                    float dy = (point2.y - point1.y) * 2f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

                    if (currentLength <= distance)
                    {
                        float t = currentLength / distance;
                        Vector3 polePosition = Vector3.Lerp(point1, point2, t);
                        polePosition.y += poleYOffset;
                        pillars[pillarIndex].position = polePosition;
                        pillarIndex++;
                        currentLength += gapLength;
                    }
                    else
                    {
                        currentLength -= distance;
                        segmentIndex++;
                    }
                }
            }
        }

        private float CalculateSegmentLength(Vector3[] segment)
        {
            float length = 0f;
            for (int i = 0; i < segment.Length - 1; i++)
            {
                Vector3 point1 = segment[i];
                Vector3 point2 = segment[i + 1];
                float dx = point2.x - point1.x;
                float dy = (point2.y - point1.y) * 2f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                length += distance;
            }
            return length;
        }

        private int CountPolesInSegment(Vector3[] segment, int startIndex)
        {
            int count = 0;
            for (int i = startIndex; i < pillars.Count - 1; i++)
            {
                if (pillars[i].position.z > segment[0].z && pillars[i].position.z < segment[segment.Length - 1].z)
                {
                    count++;
                }
            }
            return count;
        }
    }
}