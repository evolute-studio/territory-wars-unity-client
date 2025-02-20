using TerritoryWars.General;
using TerritoryWars.Tile;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button rotateTileButton;
        [SerializeField] private Image currentTilePreview;
        [SerializeField] private TextMeshProUGUI remainingTilesText;
        [SerializeField] private Button jokerButton;
        [SerializeField] private TextMeshProUGUI player1JokersText;
        [SerializeField] private TextMeshProUGUI player2JokersText;
        [SerializeField] private GameObject jokerModeIndicator;

        [Header("Tile Preview")]
        [SerializeField] private TilePreview tilePreview;
        [SerializeField] private TileJokerAnimator tilePreviewUITileJokerAnimator;

        private General.SessionManager _sessionManager;
        private DeckManager deckManager;

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

            if (jokerButton != null)
            {
                jokerButton.onClick.AddListener(OnJokerButtonClicked);
            }
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
        }

        private void OnRotateButtonClicked()
        {
            Debug.Log("Rotate button clicked");
            _sessionManager.RotateCurrentTile();
        }

        private void OnJokerButtonClicked()
        {
            _sessionManager.ActivateJoker();
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

        public void SetRotateButtonActive(bool active)
        {
            if (rotateTileButton != null)
            {
                rotateTileButton.gameObject.SetActive(active);
            }
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