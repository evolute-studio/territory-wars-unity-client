using System.Collections.Generic;
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
            CreateListItem("Player1", "Game1");
            CreateListItem("Player2", "Game2");
            CreateListItem("Player3", "Game3");
            CreateListItem("Player4", "Game4");
            CreateListItem("Player5", "Game5");
            CreateListItem("Player6", "Game6");
            CreateListItem("Player7", "Game7");
            CreateListItem("Player8", "Game8");
        }
        
        public void CreateListItem(string playerName, string gameId, UnityAction onJoin = null)
        {
            GameObject listItem = Instantiate(MatchListItemPrefab, ListItemParent);
            MatchListItem matchListItem = new MatchListItem(listItem, playerName, gameId, onJoin);
            _matchListItems.Add(matchListItem);
        }

        public void CreateMatch()
        {
            
        }
        
        public void SetActivePanel(bool isActive)
        {
            PanelGameObject.SetActive(isActive);
        }
    }

    public class MatchListItem
    {
        public GameObject ListItem;
        public string PlayerName;
        public string GameId;

        private TextMeshProUGUI _playerNameText;
        private TextMeshProUGUI _gameIdText;
        private Button _playButton;

        public MatchListItem(GameObject listItem, string playerName, string gameId, UnityAction onJoin)
        {
            ListItem = listItem;
            _playerNameText = listItem.transform.Find("Content/PlayerNameText").GetComponent<TextMeshProUGUI>();
            _gameIdText = listItem.transform.Find("Content/GameIdText").GetComponent<TextMeshProUGUI>();
            _playButton = listItem.transform.Find("Content/PlayButton").GetComponent<Button>();

            PlayerName = playerName;
            GameId = gameId;
            
            _playerNameText.text = PlayerName;
            _gameIdText.text = GameId;
            if (onJoin != null)
                _playButton.onClick.AddListener(onJoin);
        }
    }
}