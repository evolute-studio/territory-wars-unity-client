using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TerritoryWars
{

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private LayerMask backgroundLayer;

        private TileData currentTile;
        private bool isPlacingTile = false;
        private GameObject highlightedTiles;

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
            if (!deckManager.HasTiles)
            {
                Debug.Log("Гра закінчена - тайли закінчились!");
                gameUI.SetEndTurnButtonActive(false);
                gameUI.SetRotateButtonActive(false);
                return;
            }

            currentTile = deckManager.DrawTile();
            isPlacingTile = true;

            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(true);
            gameUI.UpdateUI();

            ShowPossiblePlacements();
        }

        private void ShowPossiblePlacements()
        {
            // Очищаємо попередні підсвічування
            foreach (Transform child in highlightedTiles.transform)
            {
                Destroy(child.gameObject);
            }

            if (!isPlacingTile || currentTile == null) return;

            Debug.Log($"Showing possible placements for tile {currentTile.id}");

            // Перевіряємо кожну клітинку на дошці
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (board.CanPlaceTile(currentTile, x, y))
                    {
                        CreateHighlight(x, y);
                    }
                }
            }
        }

        private void CreateHighlight(int x, int y)
        {
            Vector3 position = board.GetTilePosition(x, y);
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
            highlight.transform.parent = highlightedTiles.transform;
            highlight.transform.position = position + Vector3.back * 0.1f; // Трохи позаду тайлів
            highlight.transform.rotation = Quaternion.Euler(90, 0, 0);
            highlight.GetComponent<MeshRenderer>().material = highlightMaterial;

            // Додаємо компонент для кліку
            TileClickHandler clickHandler = highlight.AddComponent<TileClickHandler>();
            clickHandler.Initialize(this, x, y);
        }

        public void OnTileClicked(int x, int y)
        {
            Debug.Log($"OnTileClicked called with coordinates: ({x}, {y})");

            if (!isPlacingTile)
            {
                Debug.LogWarning("Not in tile placing mode");
                return;
            }

            Debug.Log($"Current tile: {currentTile?.id ?? "null"}");
            if (board.PlaceTile(currentTile, x, y))
            {
                Debug.Log($"Successfully placed tile {currentTile.id} at ({x}, {y})");
                isPlacingTile = false;
                gameUI.SetEndTurnButtonActive(true);
                gameUI.SetRotateButtonActive(false);

                // Очищаємо підсвічування
                foreach (Transform child in highlightedTiles.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                Debug.LogWarning($"Failed to place tile at ({x}, {y})");
            }
        }

        public void RotateCurrentTile()
        {
            if (currentTile != null)
            {
                Debug.Log($"Rotating tile {currentTile.id}");
                currentTile.Rotate();
                Debug.Log($"Rotated to {currentTile.id}");

                // Оновлюємо UI
                gameUI.UpdateUI();

                // Оновлюємо можливі позиції для розміщення
                ShowPossiblePlacements();
            }
        }

        public void EndTurn()
        {
            StartNewTurn();
        }
    }
}