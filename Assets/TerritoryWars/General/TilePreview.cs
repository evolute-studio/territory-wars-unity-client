using System.Collections.Generic;
using TerritoryWars.Tile;
using UnityEngine;
using DG.Tweening;

namespace TerritoryWars.General
{
    public class TilePreview : MonoBehaviour
    {
        [SerializeField] private TileGenerator tileGenerator;
        [SerializeField] private TileView previewTileView;
        [SerializeField] private float tilePreviewSetHeight = 0.5f;

        [Header("Preview Position")]
        [SerializeField] private Vector2 screenOffset = new Vector2(100f, 100f); // Відступ від правого нижнього кута

        [Header("Animation Settings")]
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutQuint;

        private Vector3 _initialPosition;
        private Tween currentTween;
        private Camera _mainCamera;

        private List<Sprite> _houseSprites = new List<Sprite>();

        private void Awake()
        {
            _mainCamera = Camera.main;
            SetInitialPosition();
        }

        private void SetInitialPosition()
        {
            // Отримуємо розміри екрану
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            // Позиція в правому нижньому куті з відступом
            Vector2 screenPosition = new Vector2(
                screenSize.x - screenOffset.x,
                screenOffset.y
            );

            // Конвертуємо позицію з екранних координат в світові
            _initialPosition = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10));
            _initialPosition.z = 0;

            // Встановлюємо початкову позицію
            transform.position = _initialPosition;
            if (previewTileView != null)
            {
                previewTileView.transform.position = _initialPosition;
            }
        }

        public void Start()
        {
            GameManager.Instance.TileSelector.OnTileSelected += SetPosition;
            GameManager.Instance.TileSelector.OnTilePlaced.AddListener(ResetPosition);

            SetupSortingLayers();
        }

        private void SetupSortingLayers()
        {
            SpriteRenderer[] spriteRenderers = previewTileView.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.sortingLayerName = "Preview";
            }

            LineRenderer[] lineRenderers = previewTileView.GetComponentsInChildren<LineRenderer>();
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                lineRenderer.sortingLayerName = "Preview";
            }
        }

        private void Update()
        {
            // Якщо тайл в початковій позиції, оновлюємо її при зміні розміру екрана
            if (transform.position == _initialPosition)
            {
                SetInitialPosition();
            }
        }

        public void UpdatePreview(TileData currentTile)
        {
            if (previewTileView != null && currentTile != null)
            {
                previewTileView.gameObject.SetActive(true);
                tileGenerator.Generate(currentTile);
                if (tileGenerator.City != null)
                {
                    Transform territoryPlacer = tileGenerator.City.transform.Find("TerritoryPlacer");
                    if (territoryPlacer != null)
                    {
                        territoryPlacer.GetComponentInChildren<SpriteMask>().frontSortingLayerID
                            = SortingLayer.NameToID("Preview");
                    }
                    _houseSprites.Clear();
                    SpriteRenderer[] houseRenderers = tileGenerator.City.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer houseRenderer in houseRenderers)
                    {
                        houseRenderer.sortingLayerName = "Preview";
                        _houseSprites.Add(houseRenderer.sprite);
                    }

                }

                previewTileView.UpdateView(currentTile);
            }
            else if (previewTileView != null)
            {
                previewTileView.gameObject.SetActive(false);
            }

            SpriteRenderer[] spriteRenderers = previewTileView.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.sortingLayerName = "Preview";
            }
            LineRenderer[] lineRenderers = previewTileView.GetComponentsInChildren<LineRenderer>();
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                lineRenderer.sortingLayerName = "Preview";
            }
        }

        public void SetPosition(int x, int y)
        {
            currentTween?.Kill();

            Vector3 targetPosition = GameManager.Instance.Board.GetTilePosition(x, y);
            targetPosition.y += tilePreviewSetHeight;

            currentTween = previewTileView.transform
                .DOMove(targetPosition, moveDuration)
                .SetEase(moveEase);
            //previewTileView.transform.DOScale(1, 0.5f).SetEase(Ease.OutQuint);
        }

        public void PlaceTile()
        {
            if (!gameObject.activeSelf) return;

            currentTween?.Kill();
            Vector3 currentPosition = previewTileView.transform.position;
            Vector3 targetPosition = currentPosition;
            targetPosition.y -= tilePreviewSetHeight;

            currentTween = previewTileView.transform
                .DOMove(targetPosition, moveDuration)
                .SetEase(moveEase)
                .OnComplete(() =>
                {
                    GameManager.Instance.TileSelector.CompleteTilePlacement();
                    // ResetPosition буде викликано через OnTilePlaced event
                });


        }

        public void ResetPosition()
        {
            currentTween?.Kill();
            currentTween = transform
                .DOMove(_initialPosition, moveDuration)
                .SetEase(moveEase);
        }

        private void OnDestroy()
        {
            // Зупиняємо всі анімації при знищенні об'єкта
            currentTween?.Kill();
        }
    }
}