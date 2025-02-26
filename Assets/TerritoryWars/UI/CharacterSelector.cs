using System;
using DG.Tweening;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class CharacterSelector : MonoBehaviour
    {
        public Character[] characters;

        public Vector3[] positions;
        public float[] scales;
        public Color[] brightness;
        public int[] order;
        public float animationDuration = 0.5f;
        private int currentCharacterId = 0;
        public Button ApplyButton;

        public void Start()
        {
            ApplyButton.onClick.AddListener(ApplyButtonClicked);
            foreach (var character in characters)
            {
                character.Initialize();
            }
        }

        public void ShiftCharacters(bool isRight)
        {
            // shift array characters
            if (isRight)
            {
                Character temp = characters[characters.Length - 1];
                for (int i = characters.Length - 1; i > 0; i--)
                {
                    characters[i] = characters[i - 1];
                }
                characters[0] = temp;
            }
            else
            {
                Character temp = characters[0];
                for (int i = 0; i < characters.Length - 1; i++)
                {
                    characters[i] = characters[i + 1];
                }
                characters[characters.Length - 1] = temp;
            }

            foreach (var character in characters)
            {
                character.Initialize();
            }

            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].CharacterRenderer.sortingOrder = order[i];
                characters[i].RockRenderer.sortingOrder = order[i] - 1;
                if (i == 1)
                {
                    Sequence sequence = DOTween.Sequence();
                    sequence.AppendInterval(animationDuration * 0.5f);
                    sequence.AppendCallback(() =>
                    {
                        Debug.Log("Play. I");
                        characters[1].CharacterAnimator.Play(characters[1].SelectedSprites);
                    });
                    sequence.AppendInterval(characters[i].CharacterAnimator.duration);
                    sequence.AppendCallback(() =>
                    {
                        characters[1].CharacterAnimator.Play(characters[1].IdleSprites);
                    });

                    sequence.Play();
                }


                characters[i].MainObject.transform
                    .DOLocalMove(positions[i], animationDuration)
                    .SetEase(Ease.InOutExpo);

                characters[i].MainObject.transform
                    .DOScale(new Vector3(scales[i], scales[i], 1), animationDuration)
                    .SetEase(Ease.InOutExpo);

                characters[i].CharacterRenderer
                    .DOColor(brightness[i], animationDuration)
                    .SetEase(Ease.InOutExpo);

                characters[i].RockRenderer
                    .DOColor(brightness[i], animationDuration)
                    .SetEase(Ease.InOutExpo);
            }
        }

        public void ApplyButtonClicked()
        {
            PlayerCharactersManager.ChangeCurrentCharacterId(characters[1].CharacterId);
        }



    }

    [Serializable]
    public class Character
    {
        public GameObject MainObject;

        public GameObject CharacterObject;
        
        public int CharacterId;
        [HideInInspector]
        public SpriteRenderer CharacterRenderer;
        [HideInInspector]
        public SpriteAnimator CharacterAnimator;

        public GameObject RockObject;
        [HideInInspector]
        public SpriteRenderer RockRenderer;
        
        public Sprite[] IdleSprites;
        public Sprite[] SelectedSprites;

        public void Initialize()
        {
            CharacterRenderer = CharacterObject.GetComponent<SpriteRenderer>();
            RockRenderer = RockObject.GetComponent<SpriteRenderer>();
            CharacterAnimator = CharacterObject.GetComponent<SpriteAnimator>();
        }
    }
}