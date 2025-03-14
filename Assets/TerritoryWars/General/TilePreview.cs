using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TerritoryWars.Tile;
using UnityEngine;
using DG.Tweening;
using TerritoryWars.UI;

namespace TerritoryWars.General
{
    public class TilePreview : MonoBehaviour
    {
        [SerializeField] private TileGenerator tileGenerator;
        [SerializeField] private TileView previewTileView;
        [SerializeField] private float tilePreviewSetHeight = 0.5f;
        public PolygonCollider2D PreviewPolygonCollider2D;
        public TileJokerAnimator _tileJokerAnimator;
        public TileJokerAnimator _tileJokerAnimatorPreview;
        
        [SerializeField] private TileGenerator tileGeneratorForUI;
        [SerializeField] private LayerMask previewLayerMask;

        [Header("Preview Position")] [SerializeField]
        private Vector2 screenOffset = new Vector2(100f, 100f);

        [Header("Animation Settings")] [SerializeField]
        private float moveDuration = 0.3f;

        [SerializeField] private Ease moveEase = Ease.OutQuint;

        private Vector3 _initialPosition;
        private Tween currentTween;
        private Camera _mainCamera;

        private List<Sprite> _houseSprites = new List<Sprite>();

        private void Awake()
        {
            _mainCamera = Camera.main;
            SetInitialPosition();
            
            tileGeneratorForUI.gameObject.SetActive(false);
        }

        private void SetInitialPosition()
        {
            
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            
            Vector2 screenPosition = new Vector2(
                screenSize.x - screenOffset.x,
                screenOffset.y
            );

            
            _initialPosition = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10));
            _initialPosition.z = 0;

            
            transform.position = _initialPosition;
            if (previewTileView != null)
            {
                previewTileView.transform.position = _initialPosition;
            }
        }

        public void Start()
        {
            SessionManager.Instance.TileSelector.OnTileSelected += SetPosition;
            SessionManager.Instance.TileSelector.OnTilePlaced.AddListener(ResetPosition);
            GameUI.OnJokerButtonClickedEvent += GenerateFFFFTile;
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
            
            if (transform.position == _initialPosition)
            {
                SetInitialPosition();
            }
        }

       

        public void UpdatePreview(TileData currentTile)
        {
            tileGeneratorForUI.gameObject.SetActive(true);
            if (previewTileView != null && currentTile != null)
            {
                previewTileView.gameObject.SetActive(true);
                tileGenerator.Generate(currentTile);
                tileGeneratorForUI.Generate(currentTile);
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

                if (tileGeneratorForUI.City != null)
                {
                    void SetLayerRecursively(Transform root)
                    {
                        root.gameObject.layer = LayerMask.NameToLayer("TilePreview");
                        foreach(Transform child in root)
                        {
                            SetLayerRecursively(child);
                        }
                    }
                    
                    SetLayerRecursively(tileGeneratorForUI.City.transform);
                    
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

        private void GenerateFFFFTile()
        {
            SessionManager.Instance.TileSelector.SetCurrentTile(new TileData("FFFF"));
            tileGenerator.Generate(new TileData("FFFF"));
            tileGeneratorForUI.Generate(new TileData("FFFF"));
        }

        public void SetPosition(int x, int y)
        {
            currentTween?.Kill();

            Vector3 targetPosition = SessionManager.Instance.Board.GetTilePosition(x, y);
            targetPosition.y += tilePreviewSetHeight;

            currentTween = previewTileView.transform
                .DOMove(targetPosition, moveDuration)
                .SetEase(moveEase);
            //previewTileView.transform.DOScale(1, 0.5f).SetEase(Ease.OutQuint);
        }
        
        public void PlaceTile(Action callback = null)
        {
            StartCoroutine(PlaceTileCoroutine(callback));
        }
        

        private IEnumerator PlaceTileCoroutine(Action callback = null)
        {
            if (!gameObject.activeSelf) yield break;
            // shake animation Y
            previewTileView.transform.DOShakePosition(0.5f, 0.1f, 18, 45, false, true);

            yield return new WaitForSeconds(0.5f);

            SpriteRenderer[] grounds =
                previewTileView.transform.Find("Ground").GetComponentsInChildren<SpriteRenderer>();
            
            foreach (SpriteRenderer ground in grounds)
            {
                ground.sortingLayerName = "Default";
            }
            
            currentTween?.Kill();
            Vector3 currentPosition = previewTileView.transform.position;
            Vector3 targetPosition = currentPosition;
            targetPosition.y -= tilePreviewSetHeight;

            currentTween = previewTileView.transform
                .DOMove(targetPosition, moveDuration)
                .SetEase(moveEase)
                .OnComplete(() =>
                {
                    SessionManager.Instance.TileSelector.CompleteTilePlacement();
                    callback?.Invoke();
                    
                });
        }

        public void ResetPosition()
        {
            currentTween?.Kill();
            // currentTween = transform
            //     .DOMove(_initialPosition, moveDuration)
            //     .SetEase(moveEase);
            tileGenerator.Generate(new TileData("FFFF"));
            _tileJokerAnimator.SetOffAllAnimationObjects();
            _tileJokerAnimatorPreview.SetOffAllAnimationObjects();
            
            transform.position = _initialPosition;
        }

        private void OnDestroy()
        {
            
            currentTween?.Kill();
            
            SessionManager.Instance.TileSelector.OnTileSelected -= SetPosition;
            GameUI.OnJokerButtonClickedEvent -= GenerateFFFFTile;
        }
    }
}