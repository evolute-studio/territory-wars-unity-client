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

        [Header("Animation Settings")]
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutQuint;

        private Vector3 _initialPosition;
        private Tween currentTween;

        private List<Sprite> _houseSprites = new List<Sprite>();

        public void Start()
        {
            GameManager.Instance.TileSelector.OnTileSelected += SetPosition;
            GameManager.Instance.TileSelector.OnTilePlaced.AddListener(ResetPosition);

            _initialPosition = previewTileView.transform.position;

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

            previewTileView.transform.DOScale(1, 0.5f).SetEase(Ease.OutQuint);
        }

        public void ResetPosition()
        {
            currentTween?.Kill();
            transform.position = _initialPosition;
            transform.localScale = Vector3.one * 2f;
        }

        private void OnDestroy()
        {
            // Зупиняємо всі анімації при знищенні об'єкта
            currentTween?.Kill();
        }
    }
}