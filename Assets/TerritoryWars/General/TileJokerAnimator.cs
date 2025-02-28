using System;
using System.Collections;
using TerritoryWars.Tools;
using UnityEngine;
using DG.Tweening;
using TerritoryWars.Tile;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Linq;
using System.Collections.Generic;

namespace TerritoryWars.General
{
    public class TileJokerAnimator : MonoBehaviour
    {
        [Header("Tile Objects")] 
        [SerializeField] private GameObject MainObject;
        [SerializeField] private GameObject[] _tileObjects;
        [SerializeField] private TileGenerator _tileGenerator;
        [SerializeField] private GameObject _evoluteTile;
        [SerializeField] private GameObject _evoluteTileDisappear;
        [SerializeField] private GameObject _explosionEffectGO;
        [SerializeField] private GameObject _JokerGroundTile;
        [SerializeField] private GameObject VFX;
        [SerializeField] private GameObject[] _lightingGO;
        [SerializeField] private GameObject _evoluteShards;
        [SerializeField] private GameObject[] _shardsEffects;
        [SerializeField] private Vector3 _shardsStartPosition;
        [SerializeField] private GameObject tileForLightning;
        public event Action OnDisappearAnimationComplete;
        public event Action OnShardsDisappearAnimationComplete;

        private Coroutine ShardEffectCoroutine;

        [Header("Settings")] 
        [SerializeField] private bool _activeStaticMember;

        private SpriteAnimator _evoluteTileSpriteAnimator;
        private static event Action CanCalculateHints;
        private SpriteRenderer _evoluteTileSpriteRenderer;
        
        private Vector3 _initialShardsParentPosition;

        private Tween _currentShardTween;
        
        
        
        private Action OnLightningEnd; 
        
        private bool _isAnimating = false;
        private Sequence _currentSequence;

        private List<Tween> _activeTweens = new List<Tween>();

        private void Awake()
        {
            Initialization();
        }

        public void Initialization()
        {
            _evoluteTileSpriteRenderer = _evoluteTile.gameObject.GetComponent<SpriteRenderer>();
            _evoluteTileSpriteAnimator = _evoluteTile.gameObject.GetComponent<SpriteAnimator>();
            _evoluteTileSpriteAnimator.Validate();
            _initialShardsParentPosition = _evoluteShards.transform.localPosition;
        }

        // when click on joker button
        public void ShowIdleJokerAnimation()
        {
            foreach (var obj in _tileObjects)   
            {
                obj.SetActive(false);
            }
                
            _evoluteTile.SetActive(true);
            _JokerGroundTile.SetActive(true);
            FindCityAndRoadsToDisable();
                
            if(_activeStaticMember)
                CanCalculateHints?.Invoke();
        }
        
        public void StopIdleJokerAnimation()
        {
            _evoluteTile.SetActive(false);
            _JokerGroundTile.SetActive(false);
            foreach (var obj in _tileObjects)
            {
                obj.SetActive(true);
            }
        }
        
        // when player place joker tile
        public void EvoluteTileDisappear()
        {
            VFX.SetActive(false);

            _evoluteTileDisappear.GetComponent<SpriteAnimator>().OnAnimationEnd = () =>
            {
                _evoluteTileDisappear.GetComponent<SpriteAnimator>().Stop();
                _evoluteTileDisappear.SetActive(false);
                _JokerGroundTile.SetActive(true);
                // _JokerGroundTile.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
                ShardAppearAnimation();
            };
            _evoluteTile.SetActive(false);
            //_JokerGroundTile.SetActive(false);
            _evoluteTileDisappear.SetActive(true);
            
            OnDisappearAnimationComplete?.Invoke();
        }

        // private IEnumerator LightningAnimation()
        // {
        //     Debug.Log("LightningAnimation");
        //     tileForLightning.SetActive(true);
        //     foreach (var lightning in _lightingGO)
        //     {
        //         lightning.GetComponent<SpriteAnimator>().OnAnimationEnd = () =>
        //         {
        //             lightning.GetComponent<SpriteAnimator>().Stop();
        //             lightning.SetActive(false);
        //         };
        //         lightning.SetActive(true);
        //         yield return new WaitForSeconds(0.2f);
        //     }
        //     
        //     tileForLightning.SetActive(false);
        //     yield return new WaitForSeconds(0.2f);
        //     OnLightningEnd?.Invoke();
        //      yield break;
        // }
        
        public void SetOffAllAnimationObjects()
        {
            _evoluteTile.SetActive(false);
            _JokerGroundTile.SetActive(false);
            _evoluteTileDisappear.SetActive(false);
            _evoluteShards.SetActive(false);
            SetActiveShardsEffects(false);
            
            foreach (var obj in _tileObjects)
            {
                obj.SetActive(true);
            }
        }
        
        // public void ShowJokerTile()
        // {
        //     _JokerGroundTile.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
        //     foreach (var obj in _tileObjects)
        //     {
        //         obj.SetActive(true);
        //     }
        //     
        //     ShardAppearAnimation();
        // }

        public void ShardAppearAnimation()
        {
            _evoluteShards.transform.localPosition = _shardsStartPosition;
            _evoluteShards.SetActive(true);
            // shard move from start point small up 
            Vector3 targetPosition = new Vector3(_shardsStartPosition.x, -0.15f, _shardsStartPosition.z);
           _evoluteShards.transform.DOLocalMove(targetPosition, 0.8f)
                .SetEase(Ease.OutQuint)
                .OnComplete(() =>
                {
                    ShardLevitationAnimation();
                    ShardEffectCoroutine = StartCoroutine(ShardEffectAnimation());
                    
                });
        }
        
        private void ShardLevitationAnimation()
        {
            SetActiveShardsEffects(true);
            Vector3 startPosition = new Vector3(_initialShardsParentPosition.x, -0.15f, 0);
            Vector3 endPosition = startPosition + new Vector3(0, -0.12f, 0);
            float duration = 0.8f;
            _currentShardTween = _evoluteShards.transform.DOLocalMove(endPosition, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private IEnumerator ShardEffectAnimation()
        {
            SetActiveShardsEffects(true);
            while (true)
            {
                foreach (var shardEffect in _shardsEffects)
                {
                    shardEffect.SetActive(true);
                    yield return new WaitForSeconds(0.5f);
                    shardEffect.SetActive(false);
                }
            }
        }
        
        public void JokerConfChanging(int x, int y)
        {
            Debug.Log("Starting JokerConfChanging");
            
            if (_isAnimating)
            {
                Debug.Log("Animation is already running");
                return;
            }
            
            CleanupActiveTweens();
            
            _isAnimating = true;

            try
            {
                Vector3 startPosition = new Vector3(_initialShardsParentPosition.x, -0.15f, 0);
                Vector3 endPosition = startPosition + new Vector3(0, -0.05f, 0);
                SetActiveShardsEffects(false);

                _currentSequence = DOTween.Sequence()
                    .OnKill(() => {
                        CleanupActiveTweens();
                        _isAnimating = false;
                    });
                    
                // First part
                if (_evoluteShards != null)
                {
                    var moveTween = _evoluteShards.transform
                        .DOLocalMove(_initialShardsParentPosition, 0.2f)
                        .SetAutoKill(true);
                    _currentSequence.Append(moveTween);
                    _activeTweens.Add(moveTween);
                }
                
                // Fade Out
                if (_tileGenerator != null && _tileGenerator.AllCityRenderers != null)
                {
                    AddFadeTweens(0.2f, 0f);
                }

                // Middle callback
                _currentSequence.AppendCallback(() => {
                    try
                    {
                        if (SessionManager.Instance != null && !_isAnimating) return;
                        
                        var jokerTile = SessionManager.Instance.GetGenerateJokerTile(x, y);
                        if (jokerTile != null)
                        {
                            SessionManager.Instance.TileSelector.StartJokerTilePlacement(jokerTile, x, y);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in joker tile generation: {e}");
                        CleanupAndReset();
                    }
                });

                // Second part
                if (_evoluteShards != null)
                {
                    var moveTween = _evoluteShards.transform
                        .DOLocalMove(endPosition, 0.3f)
                        .SetAutoKill(true);
                    _currentSequence.Append(moveTween);
                    _activeTweens.Add(moveTween);
                }
                
                // Fade In
                if (_tileGenerator != null && _tileGenerator.AllCityRenderers != null)
                {
                    AddFadeTweens(0.3f, 1f);
                }

                _currentSequence
                    .OnComplete(() => {
                        if (this == null || !gameObject.activeInHierarchy)
                        {
                            CleanupAndReset();
                            return;
                        }
                        
                        try
                        {
                            ShardLevitationAnimation();
                            SetActiveShardsEffects(true);
                        }
                        finally
                        {
                            CleanupAndReset();
                        }
                    })
                    .OnKill(() => CleanupAndReset())
                    .Play();

                Debug.Log("JokerConfChanging sequence started");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in JokerConfChanging: {e}");
                CleanupAndReset();
            }
        }
        
        private void AddFadeTweens(float duration, float targetAlpha)
        {
            try
            {
                if (_tileGenerator == null)
                    return;

                var validCityRenderers = _tileGenerator.AllCityRenderers?
                    .Where(sr => sr != null && sr.gameObject != null && sr.gameObject.activeInHierarchy)
                    .ToList() ?? new List<SpriteRenderer>();
                    
                var validLineRenderers = _tileGenerator.AllCityLineRenderers?
                    .Where(lr => lr != null && lr.gameObject != null && lr.gameObject.activeInHierarchy)
                    .ToList() ?? new List<LineRenderer>();

                foreach (var spriteRenderer in validCityRenderers)
                {
                    if (spriteRenderer == null || spriteRenderer.gameObject == null) continue;
                    
                    var tween = GetFadeSpriteRendererTween(spriteRenderer, duration, targetAlpha);
                    if (tween != null)
                    {
                        _currentSequence.Join(tween);
                        _activeTweens.Add(tween);
                    }
                }

                foreach (var lineRenderer in validLineRenderers)
                {
                    if (lineRenderer == null || lineRenderer.gameObject == null) continue;
                    
                    var tween = GetFadeLineRendererTween(lineRenderer, duration, targetAlpha);
                    if (tween != null)
                    {
                        _currentSequence.Join(tween);
                        _activeTweens.Add(tween);
                    }
                }

                if (_tileGenerator.RoadRenderer != null && 
                    _tileGenerator.RoadRenderer.gameObject != null && 
                    _tileGenerator.RoadRenderer.gameObject.activeInHierarchy)
                {
                    var roadTween = GetFadeSpriteRendererTween(_tileGenerator.RoadRenderer, duration, targetAlpha);
                    if (roadTween != null)
                    {
                        _currentSequence.Join(roadTween);
                        _activeTweens.Add(roadTween);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in AddFadeTweens: {e}");
            }
        }

        private void CleanupActiveTweens()
        {
            if (_currentSequence != null)
            {
                if (_currentSequence.IsPlaying())
                {
                    _currentSequence.Kill(true);
                }
                _currentSequence = null;
            }

            if (_currentShardTween != null)
            {
                if (_currentShardTween.IsPlaying())
                {
                    _currentShardTween.Kill(true);
                }
                _currentShardTween = null;
            }

            foreach (var tween in _activeTweens)
            {
                if (tween != null && tween.IsPlaying())
                {
                    tween.Kill(true);
                }
            }
            _activeTweens.Clear();

            if (ShardEffectCoroutine != null)
            {
                StopCoroutine(ShardEffectCoroutine);
                ShardEffectCoroutine = null;
            }
        }

        private void CleanupAndReset()
        {
            CleanupActiveTweens();
            _isAnimating = false;
        }

        private void OnDisable()
        {
            CleanupAndReset();
        }

        private void OnDestroy()
        {
            CleanupAndReset();
        }

        private Tween GetFadeSpriteRendererTween(SpriteRenderer spriteRenderer, float duration, float endAlpha)
        {
            if (spriteRenderer == null || !spriteRenderer.gameObject.activeInHierarchy)
            {
                return null;
            }
            
            try
            {
                // Зберігаємо weak reference на spriteRenderer
                var spriteRendererRef = new WeakReference<SpriteRenderer>(spriteRenderer);
                
                return DOTween.To(
                    () => {
                        if (spriteRendererRef.TryGetTarget(out var sr) && sr != null && sr.gameObject != null)
                        {
                            return sr.color.a;
                        }
                        return 0f;
                    },
                    (alpha) => {
                        if (spriteRendererRef.TryGetTarget(out var sr) && sr != null && sr.gameObject != null)
                        {
                            var color = sr.color;
                            color.a = alpha;
                            sr.color = color;
                        }
                    },
                    endAlpha,
                    duration
                )
                .SetAutoKill(true)
                .OnKill(() => {
                    if (spriteRendererRef.TryGetTarget(out var sr) && sr != null && sr.gameObject != null)
                    {
                        var color = sr.color;
                        color.a = endAlpha;
                        sr.color = color;
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating fade tween: {e}");
                return null;
            }
        }
        
        private Tween GetFadeLineRendererTween(LineRenderer lineRenderer, float duration, float endAlpha)
        {
            if (lineRenderer == null || !lineRenderer.gameObject.activeInHierarchy)
            {
                return null;
            }
            
            try
            {
                // Зберігаємо weak reference на lineRenderer
                var lineRendererRef = new WeakReference<LineRenderer>(lineRenderer);
                
                return DOTween.To(
                    () => {
                        if (lineRendererRef.TryGetTarget(out var lr) && lr != null && lr.gameObject != null)
                        {
                            return lr.colorGradient.alphaKeys[0].alpha;
                        }
                        return 0f;
                    },
                    (alpha) => {
                        if (lineRendererRef.TryGetTarget(out var lr) && lr != null && lr.gameObject != null)
                        {
                            var gradient = lr.colorGradient;
                            var alphaKeys = gradient.alphaKeys;
                            alphaKeys[0].alpha = alpha;
                            var newGradient = new Gradient();
                            newGradient.SetKeys(gradient.colorKeys, alphaKeys);
                            lr.colorGradient = newGradient;
                        }
                    },
                    endAlpha,
                    duration
                )
                .SetAutoKill(true)
                .OnKill(() => {
                    if (lineRendererRef.TryGetTarget(out var lr) && lr != null && lr.gameObject != null)
                    {
                        var gradient = lr.colorGradient;
                        var alphaKeys = gradient.alphaKeys;
                        alphaKeys[0].alpha = endAlpha;
                        var newGradient = new Gradient();
                        newGradient.SetKeys(gradient.colorKeys, alphaKeys);
                        lr.colorGradient = newGradient;
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating line renderer fade tween: {e}");
                return null;
            }
        }
        
        public void SetActiveShardsEffects(bool active)
        {
            _shardsEffects[0].transform.parent.gameObject.SetActive(active);
        }

        public void FindCityAndRoadsToDisable()
        {
            foreach (Transform child in MainObject.transform)
            {
                if (child.name.Contains("City_"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}