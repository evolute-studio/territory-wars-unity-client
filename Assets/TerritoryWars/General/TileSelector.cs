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
        [SerializeField] private TileJokerAnimator TileJokerAnimator;
        [SerializeField] private TileJokerAnimator TileJokerAnimatorUI;
       

        [Header("Colors")]
        [SerializeField] private Color normalHighlightColor = new Color(1f, 1f, 0f, 0.3f);
        [SerializeField] private Color selectedHighlightColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color invalidHighlightColor = new Color(1f, 0f, 0f, 0.3f);

        private TileData currentTile;
        private GameObject highlightedTiles;
        private List<int> currentValidRotations;
        public Vector2Int? selectedPosition;
        private bool isPlacingTile;
        private string initialTileConfig;
        private bool isJokerMode = false;
        private Vector2Int? jokerPosition;

        public TileData CurrentTile => currentTile;
        public delegate void TileSelected(int x, int y);
        public event TileSelected OnTileSelected;
        public UnityEvent OnTilePlaced = new UnityEvent();
        public UnityEvent OnTurnStarted = new UnityEvent();
        public UnityEvent OnTurnEnding = new UnityEvent();
        
        [SerializeField] private float highlightYOffset = -0.08f;

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
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                Debug.Log($"Mouse click. IsJokerMode: {isJokerMode}, SelectedPosition: {selectedPosition}");
                if (isJokerMode && selectedPosition.HasValue)
                {
                    Debug.Log("Joker mode");
                    RegenerateJokerTile();
                    return;
                }

                if (isPlacingTile)
                {
                    HandleTilePlacement();
                }
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
            highlight.transform.position = board.GetTilePosition(x, y) + new Vector3(0f, highlightYOffset, 0f);

            var spriteRenderer = highlight.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = highlightSprite;
            spriteRenderer.material = highlightMaterial;
            spriteRenderer.sortingOrder = 9;

            var clickHandler = highlight.AddComponent<TileClickHandler>();
            clickHandler.Initialize(this, x, y);
        }

        public void OnTileClicked(int x, int y)
        {
            if (isJokerMode)
            {
                if (IsValidJokerPosition(x, y))// this needs to change for animation
                {
                    tilePreview.SetPosition(x, y);
                    TileJokerAnimator.EvoluteTileDisappear();
                    TileJokerAnimatorUI.EvoluteTileDisappear();
                    TileJokerAnimator.OnDisappearAnimationComplete += () =>
                    {
                        GameManager.Instance.GenerateJokerTile(x, y);
                    };
                }
                return;
            }

            if (!isPlacingTile) return;

            if (selectedPosition.HasValue && selectedPosition.Value == new Vector2Int(x, y))
            {
                return;
            }

            if (selectedPosition.HasValue)
            {
                currentTile.SetConfig(initialTileConfig);
                selectedPosition = null;
                currentValidRotations = null;
                SetHighlightColor(normalHighlightColor);
            }

            currentValidRotations = board.GetValidRotations(currentTile, x, y);
            if (currentValidRotations.Count == 0)
            {
                return;
            }

            if (!selectedPosition.HasValue)
            {
                OnTurnStarted.Invoke();
            }

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
            try
            {
                if (!selectedPosition.HasValue) return;

                if (board.PlaceTile(currentTile, selectedPosition.Value.x, selectedPosition.Value.y, GameManager.Instance.CurrentCharacter.Id))
                {
                    isPlacingTile = false;
                    isJokerMode = false;
                    selectedPosition = null;
                    ClearHighlights();
                    OnTilePlaced.Invoke();
                    gameUI.SetEndTurnButtonActive(false);
                    gameUI.SetRotateButtonActive(false);

                    GameManager.Instance.CompleteJokerPlacement();
                    GameManager.Instance.CompleteEndTurn();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in CompleteTilePlacement: {e}");
            }
        }
        
        public void SetCurrentTile(TileData tileData)
        {
            currentTile = tileData;
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

        public void StartJokerPlacement()
        {
            isJokerMode = true;
            jokerPosition = null;



            // Показуємо можливі позиції для джокера
            ShowJokerPlacements();
        }
        
        private void ShowJokerPlacements()
        {
            ClearHighlights();
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (IsValidJokerPosition(x, y))
                    {
                        CreateHighlight(x, y);
                    }
                }
            }
            SetHighlightColor(normalHighlightColor);
        }
        
        private bool IsValidJokerPosition(int x, int y)
        {
            if (board.GetTileData(x, y) != null) return false;
            
            bool hasNonBorderNeighbor = false;
            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + board.GetXOffset(side);
                int newY = y + board.GetYOffset(side);
                
                if (board.IsValidPosition(newX, newY) && board.GetTileData(newX, newY) != null)
                {
                    if (!board.IsBorderTile(newX, newY))
                    {
                        hasNonBorderNeighbor = true;
                        break;
                    }
                }
            }
            
            return hasNonBorderNeighbor;
        }
        
        public void StartJokerTilePlacement(TileData tile, int x, int y)
        {
            try
            {
                currentTile = tile;
                selectedPosition = new Vector2Int(x, y);
                isPlacingTile = true;
                
                
                // Показуємо превью тайлу
                gameUI.UpdateUI();
                gameUI.SetEndTurnButtonActive(true);
                
                // Оновлюємо підсвічування
                ClearHighlights();
                CreateHighlight(x, y);
                SetHighlightColor(selectedHighlightColor);
                
                // Переміщуємо превью на вибрану позицію
                tilePreview.SetPosition(x, y);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in StartJokerTilePlacement: {e}");
            }
        }

        // Додамо новий метод для обробки кліку по тайлу в режимі джокера
        public void RegenerateJokerTile()
        {
            Debug.Log("RegenerateJokerTile");
            if (!isJokerMode || !selectedPosition.HasValue) return;
            Debug.Log("RegenerateJokerTile 2");
            TileJokerAnimator.OnDisappearAnimationComplete += () =>
            {
                GameManager.Instance.GenerateJokerTile(selectedPosition.Value.x, selectedPosition.Value.y);
                TileJokerAnimator.ShardAppearAnimation();
                TileJokerAnimatorUI.ShardAppearAnimation();
            };
            TileJokerAnimator.ShardsDisappear();
            TileJokerAnimatorUI.ShardsDisappear();
            
            
            
            
            gameUI.SetRotateButtonActive(currentValidRotations.Count > 1);
            gameUI.SetEndTurnButtonActive(true);
            
        }
    }
}