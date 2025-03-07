using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TerritoryWars.Tile
{
    public class TileRotator : MonoBehaviour
    {
        public List<Transform> SimpleRotationObjects = new List<Transform>();
        public List<Transform> MirrorRotationObjects = new List<Transform>();
        public List<LineRenderer> LineRenderers = new List<LineRenderer>();
        public List<SpriteSwapElement> SpriteSwapElements = new List<SpriteSwapElement>();

        public UnityEvent OnRotation;

        [Header("Debug")]
        [SerializeField] private bool autoRotate = false;
        [SerializeField] private float rotateInterval = 1f;
        private float nextRotateTime;

        [ContextMenu("Rotate Clockwise")]
        public void RotateClockwise()
        {
            ApplyRotation();
        }

        public void RotateCounterClockwise()
        {
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            foreach (var child in SimpleRotationObjects)
            {
                SimpleRotation(child);
            }

            foreach (var child in MirrorRotationObjects)
            {
                MirrorRotation(child);
            }

            foreach (var lineRenderer in LineRenderers)
            {
                LineRotation(lineRenderer);
            }
            
            foreach (var spriteSwapElement in SpriteSwapElements)
            {
                spriteSwapElement.Rotate();
            }
            OnRotation?.Invoke();
        }

        public void SimpleRotation(Transform obj, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Vector3 originalPos = obj.localPosition;
                Vector3 newPos = originalPos;

                newPos.x = originalPos.y * 2;
                newPos.y = originalPos.x / -2;
                obj.localPosition = newPos;
            }
        }

        public void MirrorRotation(Transform obj, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Vector3 originalPos = obj.localPosition;
                Vector3 newPos = originalPos;

                newPos.x = originalPos.y * 2;
                newPos.y = originalPos.x / -2;
                obj.localPosition = newPos;
                obj.localScale = new Vector3(-obj.localScale.x, obj.localScale.y, obj.localScale.z);
            }
        }

        public static void GetMirrorRotationStatic(Transform obj, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                Vector3 originalPos = obj.localPosition;
                Vector3 newPos = originalPos;

                newPos.x = originalPos.y * 2;
                newPos.y = originalPos.x / -2;
                obj.localPosition = newPos;
                obj.localScale = new Vector3(-obj.localScale.x, obj.localScale.y, obj.localScale.z);
            }
        }

        public void LineRotation(LineRenderer lineRenderer, int times = 1)
        {
            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);
            for (int t = 0; t < times; t++)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    Vector3 originalPos = positions[i];
                    Vector3 newPos = originalPos;

                    newPos.x = originalPos.y * 2;
                    newPos.y = originalPos.x / -2;
                    positions[i] = newPos;
                }
                lineRenderer.SetPositions(positions);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateClockwise();
            }

            if (autoRotate && Time.time >= nextRotateTime)
            {
                RotateClockwise();
                nextRotateTime = Time.time + rotateInterval;
            }
        }

        public void ClearLists()
        {
            SimpleRotationObjects = new List<Transform>();
            MirrorRotationObjects = new List<Transform>();
            LineRenderers = new List<LineRenderer>();
        }

        private void OnValidate()
        {
            nextRotateTime = Time.time;
        }

       
    }

    [Serializable]
    public class SpriteSwapElement
    {
        public SpriteRenderer SpriteRenderer;
        public SpriteSwapRule[] Rules;
        
        public int GetCurrentRuleIndex()
        {
            for (int i = 0; i < Rules.Length; i++)
            {
                if (SpriteRenderer.sprite == Rules[i].Sprite && SpriteRenderer.transform.localScale == Rules[i].Scale)
                {
                    return i;
                }
            }

            return -1;
        }
        
        public void Rotate(int times = 1)
        {
            int currentIndex = GetCurrentRuleIndex();
            if (currentIndex == -1) return;
            int newIndex = (currentIndex + times) % Rules.Length;
            SpriteRenderer.sprite = Rules[newIndex].Sprite;
            SpriteRenderer.transform.localScale = Rules[newIndex].Scale;
        }
    }
    
    [Serializable]
    public class SpriteSwapRule
    {
        public Sprite Sprite;
        public Vector3 Scale;
    }
}