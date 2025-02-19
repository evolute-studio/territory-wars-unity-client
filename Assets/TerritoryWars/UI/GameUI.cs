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

        private General.GameManager gameManager;
        private DeckManager deckManager;

        private void Start()
        {
            gameManager = FindObjectOfType<General.GameManager>();
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
                tilePreview.UpdatePreview(gameManager.TileSelector.CurrentTile);
            }

            // Оновлюємо текст кількості джокерів для кожного гравця
            if (player1JokersText != null)
            {
                player1JokersText.text = $"Jokers: {gameManager.GetJokerCount(0)}";
            }
            if (player2JokersText != null)
            {
                player2JokersText.text = $"Jokers: {gameManager.GetJokerCount(1)}";
            }

            // Оновлюємо стан кнопки джокера
            if (jokerButton != null)
            {
                jokerButton.interactable = gameManager.CanUseJoker();
            }

            // Оновлюємо індикатор режиму джокера
            if (jokerModeIndicator != null)
            {
                jokerModeIndicator.SetActive(gameManager.IsJokerActive);
            }
        }

        private void OnEndTurnClicked()
        {
            gameManager.EndTurn();
            UpdateUI();
        }

        private void OnRotateButtonClicked()
        {
            Debug.Log("Rotate button clicked");
            gameManager.RotateCurrentTile();
        }

        private void OnJokerButtonClicked()
        {
            gameManager.ActivateJoker();
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