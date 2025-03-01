using System;
using System.Collections.Generic;
using TerritoryWars;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class SessionUI : MonoBehaviour
{
    public Color JokerNotAvailableColor;
    public Color JokerAvailableColor;
    public List<TextMeshProUGUI> cityScoreTextPlayers;
    public List<TextMeshProUGUI> tileScoreTextPlayers;
    public List<TextMeshProUGUI> timeTextPlayers;
    [SerializeField] private Board _board;
    public CharactersObject charactersObject;
    public TextMeshProUGUI LocalPlayerName;
    public TextMeshProUGUI RemotePlayerName;
    public TextMeshProUGUI LocalPlayerScoreText;
    public TextMeshProUGUI RemotePlayerScoreText;
    public TextMeshProUGUI DeckCountText; 
    public Button CancelGameButton;
    
    
    public List<Image> imagePlayers;

    [Header("Players")]
    public List<PlayerInfo> players;
    
    private SessionManager _sessionManager;

    public void Initialization()
    {
        _sessionManager = FindObjectOfType<SessionManager>();
        SetNames(_sessionManager.PlayersData[0].username, _sessionManager.PlayersData[1].username);
        for (int i = 0; i < players.Count; i++)
        {
            cityScoreTextPlayers[i].text = players[i].cityScore.ToString();
            tileScoreTextPlayers[i].text = players[i].tileScore.ToString();
            timeTextPlayers[i].text = players[i].time.ToString();
        }

        foreach (var player in players)
        {
            foreach (var joker in player.jokersImage)
            {
                joker.color = JokerAvailableColor;
            }
        }
        CancelGameButton.onClick.AddListener(() =>
        {
            DojoGameManager.Instance.CancelGame();
            CustomSceneManager.Instance.LoadLobby();
        });
        
        SetPlayersAvatars(charactersObject.GetAvatar(PlayerCharactersManager.GetCurrentCharacterId()), 
            charactersObject.GetAvatar(PlayerCharactersManager.GetOpponentCurrentCharacterId()));
    }
    
    public void SetPlayersAvatars(Sprite localPlayerSprite, Sprite remotePlayerSprite)
    {
        players[0].playerImage.sprite = localPlayerSprite;
        players[1].playerImage.sprite = remotePlayerSprite;
    }
    

    public void SetCityScores(int localPlayerCityScore, int remotePlayerCityScore)
    {
        int[] playersCityScores = SetLocalPlayerData.GetLocalPlayerInt(localPlayerCityScore, remotePlayerCityScore);
        cityScoreTextPlayers[0].text = playersCityScores[0].ToString();
        cityScoreTextPlayers[1].text = playersCityScores[1].ToString();
    }
    
    
    public void SetRoadScores(int localPlayerTileScore, int remotePlayerTileScore)
    {
        int[] playersRoadScores = SetLocalPlayerData.GetLocalPlayerInt(localPlayerTileScore, remotePlayerTileScore);
        tileScoreTextPlayers[0].text = playersRoadScores[0].ToString();
        tileScoreTextPlayers[1].text = playersRoadScores[1].ToString();
    }

    public void SetPlayerScores(int localPlayerScore, int remotePlayerScore)
    {
        int[] playersScores = SetLocalPlayerData.GetLocalPlayerInt(localPlayerScore, remotePlayerScore);
        LocalPlayerScoreText.text = playersScores[0].ToString();
        RemotePlayerScoreText.text = playersScores[1].ToString();
    }

    // public void UpdateTime()
    // {
    //     int currentCharacter = TerritoryWars.General.SessionManager.Instance.GetCurrentCharacter();
    //     players[currentCharacter].UpdateTimer();
    //     timeTextPlayers[currentCharacter].text = string.Format("{0:00}:{1:00}",
    //         Mathf.Floor(players[currentCharacter].time / 60),
    //         Mathf.Floor(players[currentCharacter].time % 60));
    // }
    
    public void SetNames(string localPlayerName, string remotePlayerName)
    {
        string[] playerNames = SetLocalPlayerData.GetLocalPlayerString(localPlayerName, remotePlayerName);
        LocalPlayerName.text = playerNames[0];
        RemotePlayerName.text = playerNames[1];
    }

    public void UseJoker(int player)
    {
        if (!SessionManager.Instance.IsLocalPlayerHost)
        {
            player = player == 0 ? 1 : 0;
        }
        
        if (players[player].jokerCount == 0)
            return;

        players[player].jokerCount--;
        players[player].jokersImage[players[player].jokerCount].color = JokerNotAvailableColor;
        players[player].jokerCountText.text = players[player].jokerCount.ToString();
    }

    public void UpdateJokerText(int player)
    {
        if (!SessionManager.Instance.IsLocalPlayerHost)
        {
            player = player == 0 ? 1 : 0;
        }

        players[player].jokerCountText.text = players[player].jokerCount.ToString();
    }
    
    public void UpdateDeckCount()
    {
        DeckCountText.text = SessionManager.Instance.TilesInDeck.ToString();
    }

    [Serializable]
    public class PlayerInfo
    {
        public Image playerImage;
        public int jokerCount = 3;
        public int cityScore = 0;
        public int tileScore = 0;
        public float time = 600f;
        public List<Image> jokersImage;
        public TextMeshProUGUI jokerCountText;

        public void UpdateTimer()
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                time = 0;
            }
        }
    }
}
