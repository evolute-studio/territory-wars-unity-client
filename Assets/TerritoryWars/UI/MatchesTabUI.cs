using System.Collections;
using System.Collections.Generic;
using Dojo;
using Dojo.Starknet;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class MatchesTabUI : MonoBehaviour
    {
        public GameObject PanelGameObject;
        public GameObject MatchListItemPrefab;
        public Transform ListItemParent;
        
        private List<MatchListItem> _matchListItems = new List<MatchListItem>();

        public void Start() => Initialize();
        
        public void Initialize()
        {
            
            
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
        }

        private void CreatedNewEntity(GameObject newEntity)
        {
            if (!newEntity.TryGetComponent(out evolute_duel_Game gameModel)) return;
            FetchData();
        }
        
        private void ModelUpdated(ModelInstance modelInstance)
        {
            if (!modelInstance.transform.TryGetComponent(out evolute_duel_Game gameModel)) return;
            FetchData();
        }
        
        private void FetchData()
        {
            ClearAllListItems();
            GameObject[] games = DojoGameManager.Instance.GetGames();
            foreach (var game in games)
            {
                if (!game.TryGetComponent(out evolute_duel_Game gameModel)) return;
                MatchListItem matchListItem = CreateListItem();

                string playerName = gameModel.player.Hex();
                string gameId = gameModel.board_id switch
                {
                    Option<FieldElement>.Some some => some.value.Hex(),
                    Option<FieldElement>.None => "None"
                };
                string status = gameModel.status switch
                {
                    GameStatus.Created => "Created",
                    GameStatus.InProgress => "In Progress",
                    GameStatus.Finished => "Finished",
                    GameStatus.Canceled => "Canceled",
                    _ => "Unknown"
                };
                
                matchListItem.UpdateItem(playerName, gameId, status, () =>
                {
                    DojoGameManager.Instance.JoinGame(gameModel.player);
                });
                
                if (playerName == DojoGameManager.Instance.LocalBurnerAccount.Address.Hex())
                {
                    matchListItem.SetAwaiting(true);
                }
            }

            SortByStatus();
        }
        
        private void SortByStatus()
        {
            // Created -> In Progress -> Finished -> Canceled
            _matchListItems.Sort((a, b) =>
            {
                int aStatus = a.Status switch
                {
                    "Created" => 0,
                    "In Progress" => 1,
                    "Finished" => 2,
                    "Canceled" => 3,
                    _ => 4
                };
                int bStatus = b.Status switch
                {
                    "Created" => 0,
                    "In Progress" => 1,
                    "Finished" => 2,
                    "Canceled" => 3,
                    _ => 4
                };
                return aStatus - bStatus;
            });
            
            for (int i = 0; i < _matchListItems.Count; i++)
            {
                _matchListItems[i].ListItem.transform.SetSiblingIndex(i);
            }
        }

        public void CreateMatch()
        {
            DojoGameManager.Instance.CreateGame();
        }
        
        public void SetActivePanel(bool isActive)
        {
            PanelGameObject.SetActive(isActive);
            if (isActive)
            {
                FetchData();
                DojoGameManager.Instance.WorldManager.synchronizationMaster.OnEntitySpawned.AddListener(CreatedNewEntity);
                DojoGameManager.Instance.WorldManager.synchronizationMaster.OnModelUpdated.AddListener(ModelUpdated);
            }
            else
            {
                DojoGameManager.Instance.WorldManager.synchronizationMaster.OnEntitySpawned.RemoveListener(CreatedNewEntity);
                DojoGameManager.Instance.WorldManager.synchronizationMaster.OnModelUpdated.RemoveListener(ModelUpdated);
                ClearAllListItems();
            }
        }
    }

    public class MatchListItem
    {
        public GameObject ListItem;
        public string PlayerName;
        public string GameId;
        public string Status;

        private TextMeshProUGUI _playerNameText;
        //private TextMeshProUGUI _gameIdText;
        private TextMeshProUGUI _statusText;
        private TextMeshProUGUI _awaitText;
        private Button _playButton;

        public MatchListItem(GameObject listItem)
        {
            ListItem = listItem;
            _playerNameText = listItem.transform.Find("Content/PlayerNameText").GetComponent<TextMeshProUGUI>();
            //_gameIdText = listItem.transform.Find("Content/GameIdText").GetComponent<TextMeshProUGUI>();
            _statusText = listItem.transform.Find("Content/StatusText").GetComponent<TextMeshProUGUI>();
            _awaitText = listItem.transform.Find("Content/AwaitText").GetComponent<TextMeshProUGUI>();
            _playButton = listItem.transform.Find("Content/PlayButton").GetComponent<Button>();
            _awaitText.gameObject.SetActive(false);
            _awaitText.text = "Await...";
        }
        public void UpdateItem(string playerName, string gameId, string status, UnityAction onJoin = null)
        {
            PlayerName = playerName;
            //GameId = gameId;
            Status = status;
            
            _playerNameText.text = PlayerName;
            //_gameIdText.text = GameId;
            _statusText.text = Status;
            
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
        
        
    }
}