using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class MatchesTabController : MonoBehaviour
    {
        public GameObject PanelGameObject;
        public GameObject MatchListItemPrefab;
        public Transform ListItemParent;
        public TextMeshProUGUI CreatedMatchesText;
        public TextMeshProUGUI InProgressMatchesText;
        public TextMeshProUGUI FinishedMatchesText;
        public TextMeshProUGUI CanceledMatchesText;
        public GameObject BackgroundPlaceholderGO;
        public Button CreateMatchButton;
        
        private int _createdMatchesCount = 0;
        private int _inProgressMatchesCount = 0;
        private int _finishedMatchesCount = 0;
        private int _canceledMatchesCount = 0;
        
        private List<MatchListItem> _matchListItems = new List<MatchListItem>();
        private List<ModelInstance> _games = new List<ModelInstance>();

        public void Start() => Initialize();
        
        public void Initialize()
        {
            CreateMatchButton.onClick.AddListener(CreateMatch);
            
        }
        
        public MatchListItem CreateListItem()
        {
            GameObject listItem = Instantiate(MatchListItemPrefab, ListItemParent);
            MatchListItem matchListItem = new MatchListItem(listItem);
            _matchListItems.Add(matchListItem);
            return matchListItem;
        }
        
        private void ClearAllListItems()
        {
            foreach (var matchListItem in _matchListItems)
            {
                Destroy(matchListItem.ListItem);
            }
            _matchListItems.Clear();
            _createdMatchesCount = 0;
            _inProgressMatchesCount = 0;
            _finishedMatchesCount = 0;
            _canceledMatchesCount = 0;
        }
        
        public void SetBackgroundPlaceholder(bool isActive)
        {
            BackgroundPlaceholderGO.SetActive(isActive);
        }
        
        public void SetInProgressMatchesText(int count)
        {
            InProgressMatchesText.text = "Games In progress: " + count;
        }
        
        public void SetFinishedMatchesText(int count)
        {
            FinishedMatchesText.text = "Finished games: " + count;
        }
        
        public void SetCanceledMatchesText(int count)
        {
            CanceledMatchesText.text = "Canceled games: " + count;
        }
        
        private void ModelUpdated(ModelInstance modelInstance)
        {
            if (modelInstance is evolute_duel_Game && !_games.Contains(modelInstance))
            {
                FetchData();
                _games.Add(modelInstance);
            }
        }
        
        private async void FetchData()
        {
            
            ClearAllListItems();
            await DojoGameManager.Instance.SyncCreatedGames();
            GameObject[] games = DojoGameManager.Instance.GetGames();
            //BackgroundPlaceholderGO.SetActive(games.Length == 0);

            foreach (var game in games)
            {
                if (!game.TryGetComponent(out evolute_duel_Game gameModel)) return;
                // evolute_duel_Player player = await DojoGameManager.Instance.CustomSynchronizationMaster.WaitForModelByPredicate<evolute_duel_Player>(
                //     p => p.player_id.Hex() == gameModel.player.Hex()
                // );
                // if (player == null)
                // {
                //     CustomLogger.LogWarning($"Game has no player model: {gameModel.player.Hex()}");
                //     continue;
                // }
                
                evolute_duel_Player player = DojoGameManager.Instance.GetPlayerData(gameModel.player.Hex());
                string playerName = CairoFieldsConverter.GetStringFromFieldElement(player.username);
                int evoluteBalance = player.balance;
                string gameId = gameModel.board_id switch
                {
                    Option<FieldElement>.Some some => some.value.Hex(),
                    Option<FieldElement>.None => "None"
                };
                FieldElement snapshotId = gameModel.snapshot_id switch
                {
                    Option<FieldElement>.Some some => some.value,
                    Option<FieldElement>.None => null
                };
                evolute_duel_Snapshot snapshotModel = DojoGameManager.Instance.GetSnapshot(snapshotId);
                int moveNumber = snapshotModel != null ? snapshotModel.move_number : 0;
                string status = gameModel.status switch
                {
                    GameStatus.Created => "Created",
                    GameStatus.InProgress => "In Progress",
                    GameStatus.Finished => "Finished",
                    GameStatus.Canceled => "Canceled",
                    _ => "Unknown"
                };
                switch (status)
                {
                    case "Created":
                        _createdMatchesCount++;
                        break;
                    case "In Progress":
                        _inProgressMatchesCount++;
                        SetInProgressMatchesText(_inProgressMatchesCount);
                        break;
                }
                MatchListItem matchListItem = CreateListItem();
                if( status == "Created")
                {
                    matchListItem.UpdateItem(playerName, evoluteBalance, status, moveNumber,() =>
                    {
                        SetActivePanel(false);
                        DojoGameManager.Instance.JoinGame(gameModel.player);
                    });
                
                    if (playerName == DojoGameManager.Instance.LocalBurnerAccount.Address.Hex())
                    {
                        matchListItem.SetAwaiting(true);
                    }
                }
                else
                {
                    Destroy(matchListItem.ListItem);
                    _matchListItems.Remove(matchListItem);
                }
                
            }

            SetBackgroundPlaceholder(_createdMatchesCount == 0);
            SortByStatus();
        }
        
        private void SortByStatus()
        {
            // Created -> In Progress -> Finished -> Canceled
            // _matchListItems.Sort((a, b) =>
            // {
            //     int aStatus = a.Status switch
            //     {
            //         "Created" => 0,
            //         "In Progress" => 1,
            //         "Finished" => 2,
            //         "Canceled" => 3,
            //         _ => 4
            //     };
            //     int bStatus = b.Status switch
            //     {
            //         "Created" => 0,
            //         "In Progress" => 1,
            //         "Finished" => 2,
            //         "Canceled" => 3,
            //         _ => 4
            //     };
            //     return aStatus - bStatus;
            // });
            
            for (int i = 0; i < _matchListItems.Count; i++)
            {
                _matchListItems[i].ListItem.transform.SetSiblingIndex(i);
            }
        }

        public void CreateMatch()
        {
            SetActivePanel(false);
            DojoGameManager.Instance.CreateGame();
        }
        
        public async void SetActivePanel(bool isActive)
        {
            if (isActive && MenuUIController.Instance._namePanelController.IsDefaultName())
            {
                MenuUIController.Instance._changeNamePanelUIController.SetNamePanelActive(true);
                return;
            }
            
            
            PanelGameObject.SetActive(isActive);
            if (isActive)
            {
                _games = new List<ModelInstance>();
                ApplicationState.SetState(ApplicationStates.MatchTab);
                FetchData();
                IncomingModelsFilter.OnModelPassed.AddListener(ModelUpdated);
                
            }
            else
            {
                ApplicationState.SetState(ApplicationStates.Menu);
                DojoGameManager.Instance.CustomSynchronizationMaster.DestroyPlayersExceptLocal(DojoGameManager.Instance.LocalBurnerAccount.Address);
                DojoGameManager.Instance.CustomSynchronizationMaster.DestroyAllGames();
                IncomingModelsFilter.OnModelPassed.RemoveListener(ModelUpdated);
                ClearAllListItems();
            }
        }
    }

    public class MatchListItem
    {
        public GameObject ListItem;
        public string PlayerName;
        public int EvoluteCount;
        public string GameId;

        private TextMeshProUGUI _playerNameText;
        //private TextMeshProUGUI _gameIdText;
        private TextMeshProUGUI _evoluteCountText;
        private TextMeshProUGUI _moveNumberText;
        private TextMeshProUGUI _statusText;
        private TextMeshProUGUI _awaitText;
        private Button _playButton;

        public MatchListItem(GameObject listItem)
        {
            ListItem = listItem;
            _playerNameText = listItem.transform.Find("Content/PlayerNameText").GetComponent<TextMeshProUGUI>();
            //_gameIdText = listItem.transform.Find("Content/GameIdText").GetComponent<TextMeshProUGUI>();
            _evoluteCountText = listItem.transform.Find("Content/EvoluteCountGO/EvoluteCountText").GetComponent<TextMeshProUGUI>();
            _moveNumberText = listItem.transform.Find("Content/MoveNumber/MoveNumberText").GetComponent<TextMeshProUGUI>();
            _awaitText = listItem.transform.Find("Content/AwaitText").GetComponent<TextMeshProUGUI>();
            _playButton = listItem.transform.Find("Content/PlayButton").GetComponent<Button>();
            _awaitText.gameObject.SetActive(false);
            _awaitText.text = "Await...";
        }
        public void UpdateItem(string playerName, int evoluteBalance, string status, int moveNumber = 0, UnityAction onJoin = null)
        {
            PlayerName = playerName;
            EvoluteCount = evoluteBalance;
            //GameId = gameId;

            _playerNameText.text = PlayerName;
            _evoluteCountText.text = " x " + EvoluteCount.ToString();
            _moveNumberText.gameObject.SetActive(moveNumber > 0);
            _moveNumberText.text = "Move number: " + moveNumber;
            //_gameIdText.text = GameId;

            _playButton.onClick.RemoveAllListeners();

            if (status != "Created")
            {
                _playButton.interactable = false;
            }
            else
            {
                _playButton.interactable = true;
                if (onJoin != null)
                {
                    _playButton.onClick.AddListener(onJoin);
                }
            }
        }
        
        public void SetAwaiting(bool isAwaiting)
        {
            _playButton.gameObject.SetActive(!isAwaiting);
            _awaitText.gameObject.SetActive(isAwaiting);
            _awaitText.text = "Awaiting...";
        }
        
        public void SetActive(bool isActive)
        {
            ListItem.SetActive(isActive);
        }
        
        
    }
}