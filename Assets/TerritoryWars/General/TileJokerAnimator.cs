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
            //_JokerGroundTile.SetActive(false);
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
                    StartCoroutine(ShardAnimationCoroutine());
                    ShardEffectCoroutine = StartCoroutine(ShardEffectAnimation());
                });
        }
        
        private IEnumerator ShardAnimationCoroutine()
        {
            while (true)
            {
                Vector3 startPosition = _evoluteShards.transform.localPosition; 
                Vector3 endPosition = startPosition + new Vector3(0, 0.05f, 0);
                float duration = 0.8f;

                Tween shardTween = _evoluteShards.transform.DOLocalMove(endPosition, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

                yield return new WaitForSeconds(duration * 2);
            }
        }

        private IEnumerator ShardEffectAnimation()
        {
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
        
        public void ShardsDisappear()
        {
            StopCoroutine(ShardEffectCoroutine);
            _evoluteShards.transform.DOLocalMove(_shardsStartPosition, 0.2f).OnComplete(() =>
            {
                _evoluteShards.SetActive(false);
                OnShardsDisappearAnimationComplete?.Invoke();
            });
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