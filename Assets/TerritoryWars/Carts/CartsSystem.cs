using System.Collections.Generic;
using System.Linq;
using TerritoryWars.General;
using TerritoryWars.Tile;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.Carts
{
    public class CartsSystem : MonoBehaviour
    {
        public Board board;

        public static float CartsSpeed = 0.1f;
        public GameObject PlayerCartPrefab;
        
        public Sprite FirstPlayerCartSprite;
        public Sprite SecondPlayerCartSprite;

        public static Sprite FirstPlayerCartSpriteStatic;
        public static Sprite SecondPlayerCartSpriteStatic;

        // Змінюємо на Dictionary для швидкого доступу по координатах
        private Dictionary<Vector2Int, RoadTile> roadTiles = new Dictionary<Vector2Int, RoadTile>();

        public void Start() => Initialize();

        public void Initialize()
        {
            FirstPlayerCartSpriteStatic = FirstPlayerCartSprite;
            SecondPlayerCartSpriteStatic = SecondPlayerCartSprite;
            board.OnTilePlaced += OnTilePlaced;
        }

        public void OnTilePlaced(TileData tileData, int x, int y)
        {
            if (!tileData.id.Contains('R'))
            {
                return;
            }

            GameObject tileObject = board.GetTileObject(x, y);
            Transform cartsPath = tileObject.GetComponent<TileGenerator>().RoadPath.transform;
            int playerId = General.GameManager.Instance.CurrentCharacter.Id;
            RoadTile roadTile = new RoadTile(playerId, tileObject, tileData, cartsPath);
            
            //int cartsCount = tileData.id.Count(c => c == 'R');
            int cartsCount = 1;
            Cart[] carts = new Cart[cartsCount];
            for (int i = 0; i < cartsCount; i++)
            {
                GameObject cartObject = Instantiate(PlayerCartPrefab, tileObject.transform);
                cartObject.SetActive(false);
                carts[i] = new Cart(cartObject);
            }
            roadTile.AddCart(carts);

            // Додаємо тайл до словника і оновлюємо зв'язки
            Vector2Int position = new Vector2Int(x, y);
            roadTiles[position] = roadTile;
            UpdateTileConnections(roadTile, position);
        }

        private void UpdateTileConnections(RoadTile newTile, Vector2Int position)
        {
            // Отримуємо першу і останню точку шляху
            Vector3 pathStart = newTile.path[0];
            Vector3 pathEnd = newTile.path[newTile.path.Length - 1];

            // Перевіряємо всі сусідні клітинки
            Vector2Int[] neighbors = new[]
            {
                position + Vector2Int.up,
                position + Vector2Int.right,
                position + Vector2Int.down,
                position + Vector2Int.left
            };

            foreach (Vector2Int neighborPos in neighbors)
            {
                if (!roadTiles.TryGetValue(neighborPos, out RoadTile neighborTile))
                    continue;

                Vector3 neighborStart = neighborTile.path[0];
                Vector3 neighborEnd = neighborTile.path[neighborTile.path.Length - 1];

                // Перевіряємо з'єднання кінця нового тайлу з початком сусіднього
                if (Vector3.Distance(pathEnd, neighborStart) < 0.1f)
                {
                    newTile.NextTile = neighborTile;
                    neighborTile.PreviousTile = newTile;
                }
                // Перевіряємо з'єднання початку нового тайлу з кінцем сусіднього
                else if (Vector3.Distance(pathStart, neighborEnd) < 0.1f)
                {
                    newTile.PreviousTile = neighborTile;
                    neighborTile.NextTile = newTile;
                }
            }
        }

        public void Update()
        {
            foreach (var roadTile in roadTiles.Values)
            {
                roadTile.TrySpawnCarts();
                roadTile.MoveCarts();
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            foreach (var roadTile in roadTiles.Values)
            {
                roadTile.DrawCartsInfo();
            }
        }
    }

    public class RoadTile
    {
        public int OwnerId;
        public GameObject gameObject;
        public TileData tileData;
        public List<Cart> carts;
        public Vector3[] path;

        public RoadTile NextTile;
        public RoadTile PreviousTile;

        // Змінюємо на List для індексів
        private List<int> currentTargetIndices;
        public List<Cart> pendingCarts;
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
            carts = new List<Cart>();
            currentTargetIndices = new List<int>();
            OwnerId = ownerId;
        }

        public void AddCart(Cart[] newCarts)
        {
            pendingCarts = new List<Cart>(newCarts);
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
                carts[i].spriteRenderer.sprite = OwnerId == 0 ? CartsSystem.FirstPlayerCartSpriteStatic : CartsSystem.SecondPlayerCartSpriteStatic;
                if (i == 0)
                {
                    position = path[0];
                    currentTargetIndices[i] = 1; // Починаємо з другої точки
                    Debug.Log($"Cart {i}: Placing at start position {position}, Target index {currentTargetIndices[i]}");
                }
                else
                {
                    float totalLength = 0;
                    for (int j = 0; j < path.Length - 1; j++)
                    {
                        totalLength += Vector3.Distance(path[j], path[j + 1]);
                    }

                    float targetDistance = (totalLength * i) / carts.Count;
                    Debug.Log($"Cart {i}: Total path length {totalLength}, Target distance {targetDistance}");

                    float currentDistance = 0;
                    for (int j = 0; j < path.Length - 1; j++)
                    {
                        float segmentLength = Vector3.Distance(path[j], path[j + 1]);
                        if (currentDistance + segmentLength >= targetDistance)
                        {
                            float t = (targetDistance - currentDistance) / segmentLength;
                            position = Vector3.Lerp(path[j], path[j + 1], t);
                            currentTargetIndices[i] = j + 1; // Встановлюємо індекс наступної точки
                            Debug.Log($"Cart {i}: Placing between path[{j}] and path[{j + 1}] at position {position}, Target index {currentTargetIndices[i]}");
                            break;
                        }
                        currentDistance += segmentLength;
                    }
                }

                carts[i].gameObject.transform.position = position;
                carts[i].gameObject.SetActive(true);
                Debug.Log($"Cart {i}: Final position {position}, Target index {currentTargetIndices[i]}");
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

        public void AcceptCart(Cart cart)
        {
            carts.Add(cart);
            int startIndex = GetClosestPathIndex(cart.gameObject.transform.position);
            currentTargetIndices.Add(startIndex);
            cart.gameObject.transform.position = path[startIndex];
            cart.spriteRenderer.sprite = OwnerId == 0 ? CartsSystem.FirstPlayerCartSpriteStatic : CartsSystem.SecondPlayerCartSpriteStatic;
            Debug.Log($"Cart accepted at position {cart.gameObject.transform.position}, starting at index {startIndex}");
        }

        private int GetClosestPathIndex(Vector3 position)
        {
            float distanceToStart = Vector3.Distance(position, path[0]);
            float distanceToEnd = Vector3.Distance(position, path[path.Length - 1]);

            if (distanceToStart < distanceToEnd)
            {
                return 0; // Починаємо з першої точки
            }
            else
            {
                return path.Length - 1; // Починаємо з останньої точки
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

                Debug.Log($"Cart {i}: Current position {currentPosition}, Target point {targetPoint}");

                if (currentTargetIndices[i] == path.Length - 1 && Vector3.Distance(currentPosition, targetPoint) < 0.01f)
                {
                    TransferCartToNextTile(carts[i], i);
                    continue;
                }

                carts[i].gameObject.transform.position = Vector3.MoveTowards(currentPosition, targetPoint, CartsSystem.CartsSpeed * Time.deltaTime);

                if (Vector3.Distance(currentPosition, targetPoint) < 0.01f)
                {
                    currentTargetIndices[i]++;
                    Debug.Log($"Cart {i}: Reached target point, moving to next index {currentTargetIndices[i]}");
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
            Debug.Log($"Cart {cartIndex}: Next target point {nextTarget}");
            return nextTarget;
        }

        private void TransferCartToNextTile(Cart cart, int cartIndex)
        {
            if (NextTile != null)
            {
                NextTile.AcceptCart(cart);
                carts.RemoveAt(cartIndex);
                currentTargetIndices.RemoveAt(cartIndex);
            }
            else
            {
                RoadTile startingTile = FindStartingTile();
                if (startingTile != this)
                {
                    startingTile.AcceptCart(cart);
                    carts.RemoveAt(cartIndex);
                    currentTargetIndices.RemoveAt(cartIndex);
                }
                else
                {
                    // Переміщуємо карт на початок стартового тайла
                    cart.gameObject.transform.position = startingTile.path[0];
                    currentTargetIndices[cartIndex] = 0;
                    Debug.Log($"Cart transferred to start position {cart.gameObject.transform.position}, starting at index 0");
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

                // Позиція для тексту (трохи вище карта)
                Vector3 textPosition = cartPosition + Vector3.up * 0.5f;

                // Формуємо текст
                string info = $"Cart #{i}\n" +
                             $"Dir: {direction.ToString("F2")}\n" +
                             $"Target: {targetPoint.ToString("F2")}";

#if UNITY_EDITOR
                UnityEditor.Handles.BeginGUI();
                var view = UnityEditor.SceneView.currentDrawingSceneView;
                if (view != null)
                {
                    Vector3 screenPos = view.camera.WorldToScreenPoint(textPosition);
                    // Перевіряємо, чи точка перед камерою
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

                // Малюємо лінію до цільової точки
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawLine(cartPosition, targetPoint);
#endif
            }
        }
    }
}
