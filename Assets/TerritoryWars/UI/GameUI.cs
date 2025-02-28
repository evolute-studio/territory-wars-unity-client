using System;
using DG.Tweening;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        
        [Header("References")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button skipTurnButton;
        [SerializeField] private Button rotateTileButton;
        [SerializeField] private Image currentTilePreview;
        [SerializeField] private TextMeshProUGUI remainingTilesText;
        [SerializeField] private Button jokerButton;
        [SerializeField] private Button deckButton;
        [SerializeField] private TextMeshProUGUI player1JokersText;
        [SerializeField] private TextMeshProUGUI player2JokersText;
        [SerializeField] private GameObject jokerModeIndicator;
        [SerializeField] private Sprite[] _toggleMods;
        [SerializeField] private Image _toggleSpriteRenderer;
        [SerializeField] private CanvasGroup _deckContainerCanvasGroup;

        [SerializeField] private ResultPopUpUI _resultPopUpUI;
        [SerializeField] public SessionUI SessionUI;
        
        [SerializeField] private Button SaveSnapshotButton;
        [SerializeField] private TextMeshProUGUI SaveSnapshotText;
        
        public static event Action OnJokerButtonClickedEvent;

        [Header("Tile Preview")]
        [SerializeField] private TilePreview tilePreview;
        [SerializeField] private TileJokerAnimator tilePreviewUITileJokerAnimator;

        private SessionManager _sessionManager;
        private DeckManager deckManager;
        
        [SerializeField] private ArrowAnimations arrowAnimations;
        private bool _isJokerActive = false;

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

            if (jokerButton != null)
            {
                jokerButton.onClick.AddListener(OnJokerButtonClicked);
            }
            
            if (deckButton != null)
            {
                deckButton.onClick.AddListener(OnDeckButtonClicked);
            }
            
            if (SaveSnapshotButton != null)
            {
                SaveSnapshotText.gameObject.SetActive(false);
                SaveSnapshotButton.onClick.AddListener(OnSaveSnapshotButtonClicked);
            }
        }
        
        public void ShowResultPopUp()
        {
            _resultPopUpUI.SetupButtons();
            if(_sessionManager.PlayersData[0] == null || _sessionManager.PlayersData[1] == null)
            {
                CustomLogger.LogWarning("PlayersData is null");
                return;
            }
            _resultPopUpUI.SetPlayersName(_sessionManager.PlayersData[0].username, _sessionManager.PlayersData[1].username);
            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;
            int cityScoreBlue = board.blue_score.Item1;
            int cartScoreBlue = board.blue_score.Item2;
            int cityScoreRed = board.red_score.Item1;
            int cartScoreRed = board.red_score.Item2;
            int score1 = cityScoreBlue + cartScoreBlue + _sessionManager.Players[0].JokerCount * 5;
            int score2 = cityScoreRed + cartScoreRed + _sessionManager.Players[1].JokerCount * 5;
            _resultPopUpUI.SetPlayersScore(score1, score2);
            _resultPopUpUI.SetPlayersCityScores(cityScoreBlue, cityScoreRed);
            _resultPopUpUI.SetPlayersCartScores(cartScoreBlue, cartScoreRed);
            _resultPopUpUI.SetPlayersAvatars(SessionUI.charactersObject.GetAvatar(PlayerCharactersManager.GetCurrentCharacterId()),
                SessionUI.charactersObject.GetAvatar(PlayerCharactersManager.GetOpponentCurrentCharacterId()));
            _resultPopUpUI.SetPlayerHeroAnimator(SessionUI.charactersObject.GetAnimatorController(PlayerCharactersManager.GetCurrentCharacterId()),
                SessionUI.charactersObject.GetAnimatorController(PlayerCharactersManager.GetOpponentCurrentCharacterId()));
                
            bool isLocalPlayerBlue = SessionManager.Instance.LocalPlayer.LocalId == 0;
            string wonText;
            if (score1 > score2 && isLocalPlayerBlue || score1 < score2 && !isLocalPlayerBlue)
                wonText = "You won!";
            else if (score1 < score2 && isLocalPlayerBlue || score1 > score2 && !isLocalPlayerBlue)
                wonText = "You lose!";
            else
                wonText = "Draw!";
            _resultPopUpUI.SetWinnerText(wonText);
            _resultPopUpUI.SetPlayersJoker(board.GetJokerCountPlayer1(), board.GetJokerCountPlayer2());
            _resultPopUpUI.SetResultPopupActive(true);
            _resultPopUpUI.ViewResults();
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
                jokerButton.interactable = true;
                deckButton.interactable = true;
                
            }
            else
            {
                _deckContainerCanvasGroup.alpha = 1;
                _deckContainerCanvasGroup.DOFade(0.5f, 0.5f);
                jokerButton.interactable = false;
                deckButton.interactable = false;
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
            if(_isJokerActive) return;
            _isJokerActive = true;
            
            SetJokerMode(true);
            OnJokerButtonClickedEvent?.Invoke();
            
        }
        
        private void OnDeckButtonClicked()
        {
            if (!_isJokerActive) return;
            _isJokerActive = false;
            SetJokerMode(false);
        }
        
        public void SetJokerMode(bool active)
        {
            _isJokerActive = active;
            if (active)
            {
                _toggleSpriteRenderer.sprite = _toggleMods[1];
                _sessionManager.ActivateJoker();
                
                tilePreview._tileJokerAnimator.ShowIdleJokerAnimation();
                tilePreviewUITileJokerAnimator.ShowIdleJokerAnimation();
                UpdateUI();
            }
            else
            {
                _toggleSpriteRenderer.sprite = _toggleMods[0];
                _sessionManager.DeactivateJoker();
                tilePreview._tileJokerAnimator.StopIdleJokerAnimation();
                tilePreviewUITileJokerAnimator.StopIdleJokerAnimation();
                SessionManager.Instance.TileSelector.CancelJokerMode();
                UpdateUI();
            }
        }
        
        private void OnSaveSnapshotButtonClicked()
        {
            SaveSnapshotText.gameObject.SetActive(true);

            DojoGameManager.Instance.SessionManager.SetSnapshotTurn();
            
            SaveSnapshotButton.interactable = false;
            SaveSnapshotText.transform.DOScale(1.2f, 0.1f).OnComplete(() =>
            {
                SaveSnapshotText.transform.DOScale(1f, 0.1f);
            });
            DOVirtual.DelayedCall(3, () =>
            {
                SaveSnapshotButton.GetComponent<Image>().DOFade(0, 0.5f).OnComplete(() =>
                {
                    SaveSnapshotButton.gameObject.SetActive(false);
                });
                //SaveSnapshotButton.interactable = true;
                SaveSnapshotText.DOFade(0, 0.5f).OnComplete(() =>
                {
                    SaveSnapshotText.gameObject.SetActive(false);
                });
            });
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