using System;
using System.Collections;
using TerritoryWars.Tools;
using UnityEngine;
using DG.Tweening;
using TerritoryWars.Tile;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
            // SpriteAnimator _explosionEffectSpriteAnimator = _explosionEffectGO.GetComponent<SpriteAnimator>();
            // _explosionEffectGO.SetActive(true);
            // _explosionEffectSpriteAnimator.Validate();
            // _explosionEffectSpriteAnimator.OnAnimationEnd = () =>
            // {
            //     _explosionEffectSpriteAnimator.Stop();
            //     _explosionEffectGO.SetActive(false);
            //     _evoluteTile.SetActive(true);
            //     _evoluteTileSpriteAnimator.Play(_evoluteTileIdleAnimationSprites);
            // };
            // _explosionEffectSpriteAnimator.Play();
            
            
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
        
        public void ShowJokerTile()
        {
            _JokerGroundTile.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.2f);
            foreach (var obj in _tileObjects)
            {
                obj.SetActive(true);
            }
            
            ShardAppearAnimation();
        }

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
        
        public void JokerConfChanging()
        {
            _currentShardTween.Kill();
            
            Vector3 startPosition = new Vector3(_initialShardsParentPosition.x, -0.15f, 0);
            Vector3 endPosition = startPosition + new Vector3(0, -0.05f, 0);
            SetActiveShardsEffects(false);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_evoluteShards.transform.DOLocalMove(_initialShardsParentPosition, 0.2f));
            // Fade Out
            foreach (var spriteRenderer in _tileGenerator.AllCityRenderers)
            {
                sequence.Join(GetFadeSpriteRendererTween(spriteRenderer, 0.2f, 0f));
            }
            foreach (var lineRenderer in _tileGenerator.AllCityLineRenderers)
            {
                sequence.Join(GetFadeLineRendererTween(lineRenderer, 0.2f, 0f));
            }
            sequence.Join(GetFadeSpriteRendererTween(_tileGenerator.RoadRenderer, 0.2f, 0f));
            
            sequence.OnComplete(() => {
                Debug.Log("OnComplete");
                SessionManager.Instance.TileSelector.RegenerateJokerTile();
            });
            sequence.Append(_evoluteShards.transform.DOLocalMove(endPosition, 0.3f));
            // Fade In
            foreach (var spriteRenderer in _tileGenerator.AllCityRenderers)
            {
                sequence.Join(GetFadeSpriteRendererTween(spriteRenderer, 0.3f, 1f));
            }
            foreach (var lineRenderer in _tileGenerator.AllCityLineRenderers)
            {
                sequence.Join(GetFadeLineRendererTween(lineRenderer, 0.3f, 1f));
            }
            sequence.Join(GetFadeSpriteRendererTween(_tileGenerator.RoadRenderer, 0.3f, 1f));
            sequence.OnComplete(ShardLevitationAnimation);
            
            sequence.Play();
        }
        
        private Tween GetFadeSpriteRendererTween(SpriteRenderer spriteRenderer, float duration, float endAlpha)
        {
            if (spriteRenderer == null)
            {
                return null;
            }
            return spriteRenderer.DOFade(endAlpha, duration);
        }
        
        private Tween GetFadeLineRendererTween(LineRenderer lineRenderer, float duration, float endAlpha)
        {
            //lineRenderer.colorGradient.alphaKeys[0].alpha = 0;
            if (lineRenderer == null)
            {
                return null;
            }
            return DOTween.To(() => lineRenderer.colorGradient.alphaKeys[0].alpha, 
                x => lineRenderer.colorGradient.alphaKeys[0].alpha = x, endAlpha, duration);
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