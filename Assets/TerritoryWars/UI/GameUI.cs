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

        [Header("Tile Preview")]
        [SerializeField] private TilePreview tilePreview;

        private General.GameManager gameManager;
        private DeckManager deckManager;

        private void Start()
        {
            gameManager = FindObjectOfType<General.GameManager>();
            deckManager = FindObjectOfType<DeckManager>();

            SetupButtons();
            UpdateUI();
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
        }

        public void UpdateUI()
        {
            // Оновлюємо текст з кількістю тайлів
            if (remainingTilesText != null)
            {
                remainingTilesText.text = $"Tiles count: {deckManager.RemainingTiles}";
            }

            // Оновлюємо превью поточного тайлу
            if (tilePreview != null)
            {
                tilePreview.UpdatePreview(gameManager.TileSelector.CurrentTile);
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
    }
}