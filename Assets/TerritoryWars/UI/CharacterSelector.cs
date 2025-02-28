using System;
using System.Collections;
using DG.Tweening;
using TerritoryWars.General;
using TerritoryWars.Tools;
using TMPro;
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
        public TextMeshProUGUI ApplyButtonText;
        public GameObject NotActiveButton;
        public GameObject CostTextParent;
        public TextMeshProUGUI CostText;
        public GameObject AppliedText;
        public Image ApplyButtonImage;
        private bool isAnimating = false;
        [SerializeField] private float AnimationDuration = 1.5f;

        public Sprite ActiveButtonSprite;
        public Sprite DisabledButtonSprite;

        private void Initialize()
        {
            int evoluteBalance = MenuUIController.Instance._namePanelController.EvoluteBalance;
            foreach (var character in characters)
            {
                if (evoluteBalance >= character.CharacterId)
                {
                    character.Locker?.FastUnlock();
                }
                else
                {
                    character.Locker?.gameObject.SetActive(true);
                }
            }
        }

        public void Start()
        {
            ApplyButton.onClick.AddListener(ApplyButtonClicked);
            
            Initialize();
            UpdateButtons();
            foreach (var character in characters)
            {
                character.Initialize();
            }
        }

        public void ShiftCharacters(bool isRight)
        {
            // shift array characters
            if(isAnimating)
                return;
            
            isAnimating = true;
            StartCoroutine(SimpleTimer(AnimationDuration, () => { isAnimating = false; }));
            
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
                var locker = characters[i].Locker;
                if (locker != null)
                {
                    locker.GetComponent<Canvas>().sortingOrder = order[i] + 1;
                    locker.IconRenderer.color = brightness[i];
                }
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

                UpdateButtons();


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
            currentCharacterId = characters[1].CharacterId;
            PlayerCharactersManager.ChangeCurrentCharacterId(characters[1].CharacterId);
            UpdateButtons();
            characters[1].Locker?.Unlock();
            DojoGameManager.Instance.ChangePlayerSkin(characters[1].CharacterId);
        }
        
        
        private IEnumerator SimpleTimer(float duration, Action callback)
        {
            yield return new WaitForSeconds(duration);
            callback?.Invoke();
        }

        public void UpdateButtons()
        {
            Character character = characters[1];
            if (!character.Locker || character.Locker.isUnlocked)
            {
                ApplyButton.gameObject.SetActive(true);
                NotActiveButton.SetActive(false);
                CostTextParent.SetActive(false);
                if (currentCharacterId == character.CharacterId)
                {
                    ApplyButton.interactable = false;
                    ApplyButtonText.text = "APPLY";
                    ApplyButton.GetComponent<Image>().sprite = DisabledButtonSprite;
                    ApplyButton.GetComponent<Image>().color = new Color(127f / 255f, 127f / 255f, 127f / 255f, 1);
                    ApplyButton.GetComponent<CanvasGroup>().alpha = 0.74f;
                }
                else
                {
                    ApplyButton.GetComponent<Image>().sprite = ActiveButtonSprite;
                    ApplyButton.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    ApplyButton.GetComponent<CanvasGroup>().alpha = 1;
                    ApplyButton.interactable = true;
                    ApplyButtonText.text = "APPLY";
                }
            }
            else
            {
                int evoluteBalance = MenuUIController.Instance._namePanelController.EvoluteBalance;
                if (evoluteBalance >= character.Locker.cost)
                {
                    character.Locker.Unlock();
                }

                ApplyButton.gameObject.SetActive(false);
                NotActiveButton.SetActive(true);
                CostTextParent.SetActive(true);
                CostText.text = "x " + character.Locker.cost.ToString();
            }
            
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
        public Locker Locker;

        public void Initialize()
        {
            CharacterRenderer = CharacterObject.GetComponent<SpriteRenderer>();
            RockRenderer = RockObject.GetComponent<SpriteRenderer>();
            CharacterAnimator = CharacterObject.GetComponent<SpriteAnimator>();
        }
    }
}