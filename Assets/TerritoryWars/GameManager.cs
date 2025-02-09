using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TerritoryWars.Tile;
using TerritoryWars.UI;

namespace TerritoryWars
{

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private LayerMask backgroundLayer;
        [SerializeField] private Color normalHighlightColor = new Color(1f, 1f, 0f, 0.3f); // Жовтий
        [SerializeField] private Color selectedHighlightColor = new Color(0f, 1f, 0f, 0.3f); // Зелений
        [SerializeField] private Color invalidHighlightColor = new Color(1f, 0f, 0f, 0.3f); // Червоний
        [SerializeField] private Sprite highlightSprite;

        private TileData currentTile;
        private bool isPlacingTile = false;
        private GameObject highlightedTiles;
        private List<ValidPlacement> currentValidPlacements;
        private List<int> currentValidRotations;
        private Vector2Int? selectedPosition = null;

        public TileData CurrentTile => currentTile;

        private void Start()
        {
            Debug.Log($"Background layer mask value: {backgroundLayer.value}");
            Debug.Log($"BackgroundBoard layer index: {LayerMask.NameToLayer("BackgroundBoard")}");

            highlightedTiles = new GameObject("HighlightedTiles");
            highlightedTiles.transform.parent = transform;

            // Перевірка наявності колайдерів на тайлах
            var backgroundTiles = GameObject.FindGameObjectsWithTag("BackgroundTile"); // Додайте цей тег до префабу
            Debug.Log($"Found {backgroundTiles.Length} background tiles");
            foreach (var tile in backgroundTiles)
            {
                var collider = tile.GetComponent<PolygonCollider2D>();
                if (collider == null)
                {
                    Debug.LogError($"Missing BoxCollider2D on tile: {tile.name}");
                }
                Debug.Log($"Tile {tile.name} is on layer: {LayerMask.LayerToName(tile.layer)}");
            }

            StartNewTurn();
        }

        private void Update()
        {
            if (isPlacingTile && Input.GetMouseButtonDown(0))
            {
                HandleTilePlacement();
            }
        }

        private void HandleTilePlacement()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, backgroundLayer);

            Debug.Log($"Trying raycast. Mouse position: {Input.mousePosition}, Layer mask: {backgroundLayer.value}");

            if (hit.collider != null)
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name} on layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                string tileName = hit.collider.gameObject.name;
                if (tileName.StartsWith("Tile_"))
                {
                    string[] coordinates = tileName.Split('_');
                    Debug.Log($"Parsed tile name: {tileName}, coordinates length: {coordinates.Length}");

                    if (coordinates.Length == 3 &&
                        int.TryParse(coordinates[1], out int x) &&
                        int.TryParse(coordinates[2], out int y))
                    {
                        Debug.Log($"Attempting to place tile at position: ({x}, {y})");
                        OnTileClicked(x, y);
                    }
                    else
                    {
                        Debug.LogWarning("Failed to parse coordinates from tile name");
                    }
                }
                else
                {
                    Debug.LogWarning($"Hit object doesn't start with 'Tile_': {tileName}");
                }
            }
            else
            {
                Debug.LogWarning("Raycast didn't hit anything");
                // Додаткова інформація про рейкаст
                Debug.Log($"Ray origin: {ray.origin}, direction: {ray.direction}");
            }
        }

        private void StartNewTurn()
        {
            selectedPosition = null;
            if (!deckManager.HasTiles)
            {
                Debug.Log("Гра закінчена - тайли закінчились!");
                gameUI.SetEndTurnButtonActive(false);
                gameUI.SetRotateButtonActive(false);
                return;
            }

            currentTile = deckManager.DrawTile();
            isPlacingTile = true;

            // Отримуємо всі можливі позиції для розміщення
            currentValidPlacements = board.GetValidPlacements(currentTile);

            if (currentValidPlacements.Count == 0)
            {
                Debug.LogWarning("Немає можливих позицій для розміщення тайлу!");
                EndTurn();
                return;
            }

            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.UpdateUI();

            ShowPossiblePlacements();
        }

        private void ShowPossiblePlacements()
        {
            ClearHighlights();

            foreach (var placement in currentValidPlacements)
            {
                CreateHighlight(placement.X, placement.Y);
            }

            // Встановлюємо нормальний колір для всіх підсвічувань
            SetHighlightColor(normalHighlightColor);
        }

        private void CreateHighlight(int x, int y)
        {
            Vector3 position = board.GetTilePosition(x, y);
            GameObject highlight = new GameObject($"Highlight_{x}_{y}");
            highlight.transform.parent = highlightedTiles.transform;
            highlight.transform.position = position + Vector3.back * 0.1f;

            // Додаємо SpriteRenderer замість MeshRenderer
            var spriteRenderer = highlight.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = highlightSprite; // Потрібно створити SerializeField для спрайту
            spriteRenderer.material = highlightMaterial;
            spriteRenderer.sortingOrder = -1;

            TileClickHandler clickHandler = highlight.AddComponent<TileClickHandler>();
            clickHandler.Initialize(this, x, y);
        }

        public void OnTileClicked(int x, int y)
        {
            if (!isPlacingTile) return;

            // Якщо позиція вже вибрана і користувач клікнув на неї знову
            if (selectedPosition.HasValue && selectedPosition.Value.x == x && selectedPosition.Value.y == y)
            {
                PlaceTileAtPosition(x, y);
                return;
            }

            currentValidRotations = board.GetValidRotations(currentTile, x, y);

            if (currentValidRotations.Count == 0)
            {
                // Показуємо червоним, що позиція неправильна
                SetHighlightColor(invalidHighlightColor);
                selectedPosition = null;
                return;
            }

            // Оновлюємо підсвічування
            ShowPossiblePlacements();

            // Підсвічуємо тільки вибрану позицію зеленим
            foreach (Transform child in highlightedTiles.transform)
            {
                if (child.name == $"Highlight_{x}_{y}")
                {
                    child.GetComponent<SpriteRenderer>().color = selectedHighlightColor;
                }
                else
                {
                    child.GetComponent<SpriteRenderer>().color = normalHighlightColor;
                }
            }

            selectedPosition = new Vector2Int(x, y);

            // Якщо є тільки один можливий поворот, відразу розміщуємо тайл
            if (currentValidRotations.Count == 1)
            {
                while (currentTile.rotationIndex != currentValidRotations[0])
                {
                    currentTile.Rotate();
                }
                PlaceTileAtPosition(x, y);
            }
            else
            {
                // Якщо є кілька можливих поворотів, дозволяємо гравцю вибрати
                // Повертаємо тайл до першого валідного повороту
                while (currentTile.rotationIndex != currentValidRotations[0])
                {
                    currentTile.Rotate();
                }
                gameUI.SetRotateButtonActive(true);
            }
        }

        public void RotateCurrentTile()
        {
            if (currentTile == null || currentValidRotations == null || currentValidRotations.Count == 0)
                return;

            // Знаходимо наступний валідний поворот
            int currentIndex = currentValidRotations.IndexOf(currentTile.rotationIndex);
            int nextIndex = (currentIndex + 1) % currentValidRotations.Count;

            // Повертаємо тайл до наступного валідного повороту
            while (currentTile.rotationIndex != currentValidRotations[nextIndex])
            {
                currentTile.Rotate();
            }

            gameUI.UpdateUI();
        }

        private void PlaceTileAtPosition(int x, int y)
        {
            if (board.PlaceTile(currentTile, x, y))
            {
                isPlacingTile = false;
                selectedPosition = null;
                gameUI.SetEndTurnButtonActive(true);
                gameUI.SetRotateButtonActive(false);
                ClearHighlights();
            }
        }

        private void ClearHighlights()
        {
            foreach (Transform child in highlightedTiles.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void EndTurn()
        {
            StartNewTurn();
        }

        // Додайте метод для зміни кольору підсвічування
        public void SetHighlightColor(Color color)
        {
            if (highlightMaterial != null)
            {
                highlightMaterial.color = color;
            }
        }
    }
}