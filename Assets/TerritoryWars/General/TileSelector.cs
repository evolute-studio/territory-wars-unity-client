using System.Collections.Generic;
using TerritoryWars.Tile;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TerritoryWars.General
{
    public class TileSelector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Board board;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private LayerMask backgroundLayer;
        [SerializeField] private Sprite highlightSprite;
        [SerializeField] private TilePreview tilePreview;

        [Header("Colors")]
        [SerializeField] private Color normalHighlightColor = new Color(1f, 1f, 0f, 0.3f);
        [SerializeField] private Color selectedHighlightColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color invalidHighlightColor = new Color(1f, 0f, 0f, 0.3f);

        private TileData currentTile;
        private GameObject highlightedTiles;
        private List<int> currentValidRotations;
        private Vector2Int? selectedPosition;
        private bool isPlacingTile;
        private string initialTileConfig;

        public TileData CurrentTile => currentTile;
        public delegate void TileSelected(int x, int y);
        public event TileSelected OnTileSelected;
        public UnityEvent OnTilePlaced = new UnityEvent();
        public UnityEvent OnTurnStarted = new UnityEvent();
        public UnityEvent OnTurnEnding = new UnityEvent();

        private void Awake()
        {
            highlightedTiles = new GameObject("HighlightedTiles");
            highlightedTiles.transform.parent = transform;
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (isPlacingTile && Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {

                    return;
                }
                HandleTilePlacement();
            }
        }

        private void HandleTilePlacement()
        {
            var hit = Physics2D.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition).origin,
                Vector2.zero, Mathf.Infinity, backgroundLayer);

            if (hit.collider != null && hit.collider.name.StartsWith("Tile_"))
            {
                string[] coordinates = hit.collider.name.Split('_');
                if (coordinates.Length == 3 &&
                    int.TryParse(coordinates[1], out int x) &&
                    int.TryParse(coordinates[2], out int y))
                {
                    OnTileClicked(x, y);
                }
            }
        }

        public void StartTilePlacement(TileData tile)
        {
            currentTile = tile;
            initialTileConfig = tile.GetConfig();
            isPlacingTile = true;
            selectedPosition = null;

            var validPlacements = board.GetValidPlacements(currentTile);
            if (validPlacements.Count == 0)
            {
                EndTilePlacement();
                return;
            }

            ShowPossiblePlacements(validPlacements);
            gameUI.SetEndTurnButtonActive(false);
            gameUI.SetRotateButtonActive(false);
            gameUI.UpdateUI();
        }

        private void ShowPossiblePlacements(List<ValidPlacement> placements)
        {
            ClearHighlights();
            foreach (var placement in placements)
            {
                CreateHighlight(placement.X, placement.Y);
            }
            SetHighlightColor(normalHighlightColor);
        }

        private void CreateHighlight(int x, int y)
        {
            var highlight = new GameObject($"Highlight_{x}_{y}");
            highlight.transform.SetParent(highlightedTiles.transform);
            highlight.transform.position = board.GetTilePosition(x, y) + Vector3.back * 0.1f;

            var spriteRenderer = highlight.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = highlightSprite;
            spriteRenderer.material = highlightMaterial;
            spriteRenderer.sortingOrder = 9;

            var clickHandler = highlight.AddComponent<TileClickHandler>();
            clickHandler.Initialize(this, x, y);
        }

        public void OnTileClicked(int x, int y)
        {
            if (!isPlacingTile) return;

            if (selectedPosition.HasValue && selectedPosition.Value == new Vector2Int(x, y))
            {
                return;
            }

            if (!selectedPosition.HasValue)
            {
                OnTurnStarted.Invoke();
            }

            if (selectedPosition.HasValue)
            {
                currentTile.SetConfig(initialTileConfig);
                selectedPosition = null;
                currentValidRotations = null;
                SetHighlightColor(normalHighlightColor);
            }

            currentValidRotations = board.GetValidRotations(currentTile, x, y);

            if (currentValidRotations.Count > 1)
            {
                HashSet<string> uniqueConfigs = new HashSet<string>();
                List<int> uniqueRotations = new List<int>();

                foreach (int rotation in currentValidRotations)
                {
                    string currentConfig = currentTile.GetConfig();

                    while (currentTile.rotationIndex != rotation)
                    {
                        currentTile.Rotate();
                    }

                    string config = currentTile.id;
                    if (!uniqueConfigs.Contains(config))
                    {
                        uniqueConfigs.Add(config);
                        uniqueRotations.Add(rotation);
                    }

                    currentTile.SetConfig(currentConfig);
                }

                currentValidRotations = uniqueRotations;
            }

            if (currentValidRotations.Count == 0)
            {
                SetHighlightColor(invalidHighlightColor);
                selectedPosition = null;
                gameUI.SetEndTurnButtonActive(false);
                gameUI.SetRotateButtonActive(false);
                return;
            }

            UpdateHighlights(x, y);
            selectedPosition = new Vector2Int(x, y);
            RotateToFirstValidRotation();

            OnTileSelected?.Invoke(x, y);
            gameUI.SetRotateButtonActive(currentValidRotations.Count > 1);
            gameUI.SetEndTurnButtonActive(true);
        }

        private void UpdateHighlights(int selectedX, int selectedY)
        {
            foreach (Transform child in highlightedTiles.transform)
            {
                child.GetComponent<SpriteRenderer>().color =
                    child.name == $"Highlight_{selectedX}_{selectedY}"
                        ? selectedHighlightColor
                        : normalHighlightColor;
            }
        }

        private void RotateToFirstValidRotation()
        {
            if (currentValidRotations?.Count > 0)
            {
                while (currentTile.rotationIndex != currentValidRotations[0])
                {
                    currentTile.Rotate();
                }
                gameUI.UpdateUI();
            }
        }

        private void RecalculateValidRotations()
        {
            if (!selectedPosition.HasValue) return;

            currentValidRotations = board.GetValidRotations(currentTile, selectedPosition.Value.x, selectedPosition.Value.y);
            if (currentValidRotations.Count == 0)
            {
                SetHighlightColor(invalidHighlightColor);
                selectedPosition = null;
                gameUI.SetEndTurnButtonActive(false);
                gameUI.SetRotateButtonActive(false);
                return;
            }

            RotateToFirstValidRotation();
            gameUI.SetRotateButtonActive(currentValidRotations.Count > 1);
        }

        public void RotateCurrentTile()
        {
            if (currentValidRotations?.Count <= 1) return;

            int currentIndex = currentValidRotations.IndexOf(currentTile.rotationIndex);

            if (currentIndex == -1)
            {
                RecalculateValidRotations();
                return;
            }

            int nextIndex = (currentIndex + 1) % currentValidRotations.Count;
            while (currentTile.rotationIndex != currentValidRotations[nextIndex])
            {
                currentTile.Rotate();
            }

            if (!board.CanPlaceTile(currentTile, selectedPosition.Value.x, selectedPosition.Value.y))
            {
                RecalculateValidRotations();
                return;
            }

            gameUI.UpdateUI();
        }

        public void PlaceCurrentTile()
        {
            if (!selectedPosition.HasValue) return;

            OnTurnEnding.Invoke();

            gameUI.SetRotateButtonActive(false);

            tilePreview.PlaceTile();
        }

        public void CompleteTilePlacement(List<Sprite> houses = default)
        {
            if (!selectedPosition.HasValue) return;

            if (board.PlaceTile(currentTile, selectedPosition.Value.x, selectedPosition.Value.y))
            {
                isPlacingTile = false;
                selectedPosition = null;
                ClearHighlights();
                OnTilePlaced.Invoke();
                gameUI.SetEndTurnButtonActive(false);
                gameUI.SetRotateButtonActive(false);

                GameManager.Instance.CompleteEndTurn();
            }
        }

        private void ClearHighlights()
        {
            if (highlightedTiles == null) return;

            foreach (Transform child in highlightedTiles.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void EndTilePlacement()
        {
            isPlacingTile = false;
            ClearHighlights();
        }

        private void SetHighlightColor(Color color)
        {
            if (highlightMaterial != null)
            {
                highlightMaterial.color = color;
            }
        }
    }
}