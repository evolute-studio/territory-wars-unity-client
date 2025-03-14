using System.Collections.Generic;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.Carts
{
    
    public class RoadTile
    {
        public int OwnerId;
        public GameObject gameObject;
        public TileData tileData;
        public List<OwnerPin> carts;
        public Vector3[] path;

        public RoadTile NextTile;
        public RoadTile PreviousTile;
        
        private List<int> currentTargetIndices;
        public List<OwnerPin> pendingCarts;
        private bool firstCondition = false;
        private bool isReadyToSpawnCarts = false;
        private bool isCartsSpawned = false;

        public RoadTile(int ownerId, GameObject gameObject, TileData tileData, Transform pathParent)
        {
            this.gameObject = gameObject;
            this.tileData = tileData;
            path = new Vector3[pathParent.childCount];
            for (int i = 0; i < path.Length; i++)
            {
                path[i] = pathParent.GetChild(i).position;
            }

            carts = new List<OwnerPin>();
            currentTargetIndices = new List<int>();
            OwnerId = ownerId;
        }
        
        
        public void AddCart(OwnerPin[] newCarts)
        {
            pendingCarts = new List<OwnerPin>(newCarts);
        }

        public void TrySpawnCarts()
        {
            if (isCartsSpawned)
                return;

            if (PreviousTile == null)
            {
                SpawnCarts();
                isCartsSpawned = true;
            }


            if (carts.Count > 0)
            {
                firstCondition = true;
            }


            if (firstCondition && carts.Count == 0 && !isReadyToSpawnCarts)
            {
                isReadyToSpawnCarts = true;
            }


            if (isReadyToSpawnCarts && pendingCarts.Count > 0)
            {
                SpawnCarts();
                isCartsSpawned = true;
            }

        }

        private void SpawnCarts()
        {
            foreach (var cart in pendingCarts)
            {
                carts.Add(cart);
                cart.gameObject.SetActive(true);
                currentTargetIndices.Add(0);
            }

            PlaceCarts();
        }

       
        public void PlaceCarts()
        {
            for (int i = 0; i < carts.Count; i++)
            {
                if (carts[i] == null)
                    continue;

                Vector3 position = Vector3.zero;
                carts[i].spriteRenderer.sprite = OwnerId == 0
                    ? CartsSystem.FirstPlayerCartSpriteStatic
                    : CartsSystem.SecondPlayerCartSpriteStatic;
                if (i == 0)
                {
                    position = path[0];
                    currentTargetIndices[i] = 1;
                }
                else
                {
                    float totalLength = 0;
                    for (int j = 0; j < path.Length - 1; j++)
                    {
                        totalLength += Vector3.Distance(path[j], path[j + 1]);
                    }

                    float targetDistance = (totalLength * i) / carts.Count;

                    float currentDistance = 0;
                    for (int j = 0; j < path.Length - 1; j++)
                    {
                        float segmentLength = Vector3.Distance(path[j], path[j + 1]);
                        if (currentDistance + segmentLength >= targetDistance)
                        {
                            float t = (targetDistance - currentDistance) / segmentLength;
                            position = Vector3.Lerp(path[j], path[j + 1], t);
                            currentTargetIndices[i] = j + 1;
                            break;
                        }

                        currentDistance += segmentLength;
                    }
                }

                carts[i].gameObject.transform.position = position;
                carts[i].gameObject.SetActive(true);
            }
        }
        
        private RoadTile FindStartingTile()
        {
            RoadTile current = this;
            while (current.PreviousTile != null)
            {
                current = current.PreviousTile;
            }

            return current;
        }

        
        public void AcceptCart(OwnerPin ownerPin)
        {
            carts.Add(ownerPin);
            int startIndex = GetClosestPathIndex(ownerPin.gameObject.transform.position);
            currentTargetIndices.Add(startIndex);
            if (PreviousTile == null) ownerPin.gameObject.transform.position = path[startIndex];
            
            Debug.Log(
                $"Cart accepted at position {ownerPin.gameObject.transform.position}, starting at index {startIndex}");
            ownerPin.spriteRenderer.sprite = OwnerId == 0
                ? CartsSystem.FirstPlayerCartSpriteStatic
                : CartsSystem.SecondPlayerCartSpriteStatic;

        }
        
        private int GetClosestPathIndex(Vector3 position)
        {
            float distanceToStart = Vector3.Distance(position, path[0]);
            float distanceToEnd = Vector3.Distance(position, path[path.Length - 1]);

            if (distanceToStart < distanceToEnd)
            {
                return 0; 
            }
            else
            {
                return path.Length - 1; 
            }
        }

        public void MoveCarts()
        {
            for (int i = carts.Count - 1; i >= 0; i--)
            {
                if (carts[i] == null)
                {
                    carts.RemoveAt(i);
                    currentTargetIndices.RemoveAt(i);
                    continue;
                }

                Vector3 currentPosition = carts[i].gameObject.transform.position;
                Vector3 targetPoint = GetNextTargetPoint(i);

                if (currentTargetIndices[i] == path.Length - 1 &&
                    Vector3.Distance(currentPosition, targetPoint) < 0.01f)
                {
                    TransferCartToNextTile(carts[i], i);
                    continue;
                }

                carts[i].gameObject.transform.position = Vector3.MoveTowards(currentPosition, targetPoint,
                    CartsSystem.CartsSpeed * Time.deltaTime);

                if (Vector3.Distance(currentPosition, targetPoint) < 0.01f)
                {
                    currentTargetIndices[i]++;
                }
            }
        }

        public void RefreshCartDirection()
        {
            for (int i = 0; i < carts.Count; i++)
            {
                Vector3 target = GetNextTargetPoint(i);
                Vector3 direction = (target - carts[i].gameObject.transform.position).normalized;

                float maxDotProduct = float.MinValue;
                Vector3 closestDirection = Vector3.zero;

                foreach (var cartAnimator in OwnerId == 0
                             ? CartsSystem.FirstPlayerCartSpritesStatic
                             : CartsSystem.SecondPlayerCartSpritesStatic)
                {
                    float dotProduct = Vector3.Dot(direction, cartAnimator.Direction);

                    if (dotProduct > maxDotProduct)
                    {
                        maxDotProduct = dotProduct;
                        closestDirection = cartAnimator.Direction;
                    }
                }

                if (carts[i].Direction != (Vector2)closestDirection)
                {
                    carts[i].Direction = closestDirection;

                    foreach (var cartAnimator in OwnerId == 0
                                 ? CartsSystem.FirstPlayerCartSpritesStatic
                                 : CartsSystem.SecondPlayerCartSpritesStatic)
                    {
                        if (cartAnimator.Direction == closestDirection)
                        {
                            if (carts[i].gameObject.GetComponent<SpriteAnimator>().sprites == null)
                            {
                                carts[i].gameObject.GetComponent<SpriteAnimator>()
                                    .Play(cartAnimator.CartSprites.ToArray());
                            }

                            carts[i].gameObject.GetComponent<SpriteAnimator>()
                                .ChangeSprites(cartAnimator.CartSprites.ToArray());

                            if (cartAnimator.IsMirrored)
                            {
                                carts[i].gameObject.transform.localScale = new Vector3(-1, 1, 1);
                            }
                            else
                            {
                                carts[i].gameObject.transform.localScale = new Vector3(1, 1, 1);
                            }
                        }
                    }
                }
            }
        }


        private Vector3 GetNextTargetPoint(int cartIndex)
        {
            if (currentTargetIndices[cartIndex] >= path.Length)
            {
                Debug.LogWarning($"Cart {cartIndex}: Target index out of bounds, returning last path point");
                return path[path.Length - 1];
            }

            Vector3 nextTarget = path[currentTargetIndices[cartIndex]];

            return nextTarget;
        }

        private void TransferCartToNextTile(OwnerPin ownerPin, int cartIndex)
        {
            if (NextTile != null)
            {
                NextTile.AcceptCart(ownerPin);
                carts.RemoveAt(cartIndex);
                currentTargetIndices.RemoveAt(cartIndex);
            }
            else
            {
                RoadTile startingTile = FindStartingTile();
                if (startingTile != this)
                {
                    startingTile.AcceptCart(ownerPin);
                    carts.RemoveAt(cartIndex);
                    currentTargetIndices.RemoveAt(cartIndex);
                }
                else
                {
                    
                    ownerPin.gameObject.transform.position = startingTile.path[0];
                    currentTargetIndices[cartIndex] = 0;
                }
            }
        }

        public void DrawCartsInfo()
        {
            for (int i = 0; i < carts.Count; i++)
            {
                if (carts[i] == null) continue;

                Vector3 cartPosition = carts[i].gameObject.transform.position;
                Vector3 targetPoint = GetNextTargetPoint(i);
                Vector3 direction = (targetPoint - cartPosition).normalized;

                
                Vector3 textPosition = cartPosition + Vector3.up * 0.5f;

                
                string info = $"Cart #{i}\n" +
                              $"Dir: {direction.ToString("F2")}\n" +
                              $"Target: {targetPoint.ToString("F2")}";

#if UNITY_EDITOR
                UnityEditor.Handles.BeginGUI();
                var view = UnityEditor.SceneView.currentDrawingSceneView;
                if (view != null)
                {
                    Vector3 screenPos = view.camera.WorldToScreenPoint(textPosition);
                    
                    if (screenPos.z > 0)
                    {
                        screenPos.y = view.camera.pixelHeight - screenPos.y;
                        var style = new GUIStyle();
                        style.normal.textColor = Color.white;
                        style.alignment = TextAnchor.UpperLeft;
                        style.fontSize = 12;
                        UnityEditor.Handles.Label(textPosition, info, style);
                    }
                }

                UnityEditor.Handles.EndGUI();
                
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawLine(cartPosition, targetPoint);
#endif
            }
        }
    }
}