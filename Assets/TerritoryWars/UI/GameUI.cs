using System;
using DG.Tweening;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button skipTurnButton;
        [SerializeField] private Button rotateTileButton;
        [SerializeField] private Image currentTilePreview;
        [SerializeField] private TextMeshProUGUI remainingTilesText;
        [SerializeField] private Button jokerButton;
        [SerializeField] private TextMeshProUGUI player1JokersText;
        [SerializeField] private TextMeshProUGUI player2JokersText;
        [SerializeField] private GameObject jokerModeIndicator;
        [SerializeField] private Sprite[] _toggleMods;
        [SerializeField] private Image _toggleSpriteRenderer;
        [SerializeField] private CanvasGroup _deckContainerCanvasGroup;
        
        public static event Action OnJokerButtonClickedEvent;

        [Header("Tile Preview")]
        [SerializeField] private TilePreview tilePreview;
        [SerializeField] private TileJokerAnimator tilePreviewUITileJokerAnimator;

        private SessionManager _sessionManager;
        private DeckManager deckManager;
        
        [SerializeField] private ArrowAnimations arrowAnimations;

        public void Initialize()
        {
            _sessionManager = FindObjectOfType<General.SessionManager>();
            deckManager = FindObjectOfType<DeckManager>();

            SetupButtons();
            UpdateUI();
            
            // Початково ховаємо індикатор режиму джокера
            if (jokerModeIndicator != null)
            {
                jokerModeIndicator.SetActive(false);
            }
        }

        private void SetupButtons()
        {
            if (rotateTileButton != null)
            {
                rotateTileButton.onClick.AddListener(OnRotateButtonClicked);
                Debug.Log("Rotate button listener added");
            }
            else
            {
                Debug.LogError("Rotate button reference is missing!");
            }

            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }
            
            if (skipTurnButton != null)
            {
                skipTurnButton.onClick.AddListener(SkipMoveButtonClicked);
            }

            // if (jokerButton != null)
            // {
            //     jokerButton.onClick.AddListener(OnJokerButtonClicked);
            // }
        }

        public void UpdateUI()
        {
            // Оновлюємо текст з кількістю тайлів
            if (remainingTilesText != null)
            {
                remainingTilesText.text = $"{deckManager.RemainingTiles}";
            }

            // Оновлюємо превью поточного тайлу
            if (tilePreview != null)
            {
                tilePreview.UpdatePreview(_sessionManager.TileSelector.CurrentTile);
            }

            // Оновлюємо текст кількості джокерів для кожного гравця
            if (player1JokersText != null)
            {
                player1JokersText.text = $"Jokers: {_sessionManager.GetJokerCount(0)}";
            }
            if (player2JokersText != null)
            {
                player2JokersText.text = $"Jokers: {_sessionManager.GetJokerCount(1)}";
            }

            // Оновлюємо стан кнопки джокера
            if (jokerButton != null)
            {
                jokerButton.interactable = _sessionManager.CanUseJoker();
            }

            // Оновлюємо індикатор режиму джокера
            if (jokerModeIndicator != null)
            {
                jokerModeIndicator.SetActive(_sessionManager.IsJokerActive);
            }
        }

        private void OnEndTurnClicked()
        {
            _sessionManager.EndTurn();
            UpdateUI();
            SetActiveDeckContainer(false);
        }
        
        private void SkipMoveButtonClicked()
        {
            _sessionManager.SkipMove();
            UpdateUI();
        }
        
        public void SetActiveDeckContainer(bool active)
        {
            if (active)
            {
                _deckContainerCanvasGroup.alpha = 0.5f;
                _deckContainerCanvasGroup.DOFade(1, 0.5f);
                
            }
            else
            {
                _deckContainerCanvasGroup.alpha = 1;
                _deckContainerCanvasGroup.DOFade(0.5f, 0.5f);
            }
        }

        private void OnRotateButtonClicked()
        {
            Debug.Log("Rotate button clicked");
            arrowAnimations.PlayRotationAnimation();
            _sessionManager.RotateCurrentTile();
        }

        private void OnJokerButtonClicked()
        {
            OnJokerButtonClickedEvent?.Invoke();
            _sessionManager.ActivateJoker();
            SwitchToggle();
            tilePreview._tileJokerAnimator.ShowIdleJokerAnimation();
            tilePreviewUITileJokerAnimator.ShowIdleJokerAnimation();
            UpdateUI();
        }

        public void SetEndTurnButtonActive(bool active)
        {
            if (endTurnButton != null)
            {
                endTurnButton.gameObject.SetActive(active);
            }
        }
        
        public void SetSkipTurnButtonActive(bool active)
        {
            if (skipTurnButton != null)
            {
                skipTurnButton.gameObject.SetActive(active);
            }
        }

        public void SetRotateButtonActive(bool active)
        {
            if (rotateTileButton != null)
            {
                rotateTileButton.gameObject.SetActive(active);
                arrowAnimations.gameObject.SetActive(active);
                arrowAnimations.SetActiveArrow(active);
            }
        }

        private void SwitchToggle()
        {
            _toggleSpriteRenderer.sprite = _toggleMods[0] ? _toggleMods[1] : _toggleMods[0];
        }

        public void SetJokerButtonActive(bool active)
        {
            if (jokerButton != null)
            {
                jokerButton.gameObject.SetActive(active);
            }
        }
    }
}