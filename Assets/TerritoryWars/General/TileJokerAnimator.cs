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
        [SerializeField] private GameObject[] _tileObjects;
        [SerializeField]private TileGenerator _tileGenerator;
        [SerializeField] private GameObject _evoluteTile;
        [SerializeField] private GameObject _explosionEffectGO;
        [SerializeField] private GameObject _JokerGroundTile;
        [SerializeField] private GameObject VFX;
        [SerializeField] private GameObject[] _lightingGO;
        [SerializeField] private Sprite[] _evoluteTileIdleAnimationSprites;
        [SerializeField] private Sprite[] _evoluteTileDisappearAnimationSprites;

        private SpriteAnimator _evoluteTileSpriteAnimator;
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
            SpriteAnimator _explosionEffectSpriteAnimator = _explosionEffectGO.GetComponent<SpriteAnimator>();
            _explosionEffectGO.SetActive(true);
            _explosionEffectSpriteAnimator.Validate();
            _explosionEffectSpriteAnimator.OnAnimationEnd = () =>
            {
                _explosionEffectSpriteAnimator.Stop();
                _explosionEffectGO.SetActive(false);
                _evoluteTile.SetActive(true);
                _evoluteTileSpriteAnimator.Play(_evoluteTileIdleAnimationSprites);
            };
            _explosionEffectSpriteAnimator.Play();
        }
        
        // when player place joker tile
        public void FirstTransition()
        {
            VFX.SetActive(false);
            OnLightningEnd = SecondTransition;
            
            _evoluteTileSpriteAnimator.OnAnimationEnd = () =>
            {
                _evoluteTile.SetActive(false);
                _JokerGroundTile.SetActive(true);
                foreach (var obj in _tileObjects)
                {
                    obj.SetActive(false);
                }
                
                StartCoroutine(LightningAnimation());
            };
            _evoluteTileSpriteAnimator.ChangeSprites(_evoluteTileDisappearAnimationSprites);
            
        }

        private IEnumerator LightningAnimation()
        {
            for(int i = 0; i < _lightingGO.Length; i++)
            {
                _lightingGO[i].GetComponent<SpriteAnimator>().OnAnimationEnd = () =>
                {
                    _lightingGO[i].SetActive(false);
                };
                _lightingGO[i].SetActive(true);
                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(0.2f);
            OnLightningEnd?.Invoke();
        }
        
        public void SetOffAllAnimationObjects()
        {
            _evoluteTile.SetActive(false);
            _JokerGroundTile.SetActive(false);
            foreach (var obj in _tileObjects)
            {
                obj.SetActive(true);
            }
        }
        
        public void SecondTransition()
        {
            _JokerGroundTile.GetComponent<SpriteRenderer>().DOColor(new Color(255, 255, 255, 0.2f),0.5f).OnComplete(
                () =>
                {
                    char[] sides = new char[4];
                    for (int i = 0; i < 4; i++)
                    {
                        sides[i] = GetRandomLandscape();
                    }
                    string tileConfig = new string(sides);
                    TileData jokerTile = new TileData(tileConfig);
                    
                    _tileGenerator.Generate(jokerTile);
                });
        }
        
        private char GetRandomLandscape()
        {
            float random = Random.value;
            if (random < 0.4f) return 'F';      // 40% шанс поля
            else if (random < 0.7f) return 'R';  // 30% шанс дороги
            else return 'C';                     // 30% шанс міста
        }
    }
}