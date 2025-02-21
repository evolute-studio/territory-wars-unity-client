using System;
using System.Collections;
using UnityEngine;

namespace TerritoryWars.Tools
{

    public class SpriteAnimator : MonoBehaviour
    {
        public Sprite[] sprites = new Sprite[0];
        public float duration = 1f;
        public bool loop = true;
        public float delay = 0f;
        public bool randomizeStart = false;
        public float maxRandomDelay = 0.5f;
        public float waitBetweenLoops = 0f;
        public bool playOnAwake = true;
        public Action OnAnimationEnd;

        [SerializeField] private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (playOnAwake)
            {
                Play();
            }
        }

        public void Validate()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void ChangeSprites(Sprite[] AnimationSprites)
        {
            sprites = AnimationSprites;
        }

        public void Play()
        {
            Stop();
            if (sprites == null || sprites.Length == 0 || _spriteRenderer == null)
            {
                Debug.LogWarning("No sprites to animate");
                return;
            }

            if (sprites.Length == 1)
            {
                _spriteRenderer.sprite = sprites[0];
                return;
            }

            StartCoroutine(Animate());
        }

        public void Play(Sprite[] animation)
        {
            sprites = animation;
            Play();
        }

        public void Stop()
        {
            StopAllCoroutines();
        }

        private IEnumerator Animate()
        {
            if (randomizeStart)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0f, maxRandomDelay));
            }
            else if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

            while (true)
            {
                foreach (var sprite in sprites)
                {
                    _spriteRenderer.sprite = sprite;
                    yield return new WaitForSeconds(duration / sprites.Length);
                }
                
                OnAnimationEnd?.Invoke();

                if (!loop)
                {
                    break;
                }
                
                

                if (waitBetweenLoops > 0)
                {
                    yield return new WaitForSeconds(waitBetweenLoops);
                }
            }
        }

        public void OnEnable()
        {
            Play();
        }

        public void OnDisable()
        {
            Stop();
        }
    }
}