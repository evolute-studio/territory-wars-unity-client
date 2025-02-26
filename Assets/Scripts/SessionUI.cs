using System;
using System.Collections.Generic;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
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
    public TextMeshProUGUI LocalPlayerName;
    public TextMeshProUGUI RemotePlayerName;
    public CharactersObject charactersObject;
    public TextMeshProUGUI LocalPlayerScore;
    public TextMeshProUGUI RemotePlayerScore;

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
            players[i].playerImage.sprite = imagePlayers[i].sprite;
        }

        foreach (var player in players)
        {
            foreach (var joker in player.jokersImage)
            {
                joker.color = JokerAvailableColor;
            }
        }

        players[0].playerImage.sprite = charactersObject.GetAvatar(PlayerCharactersManager.GetCurrentCharacterId());
        players[1].playerImage.sprite = charactersObject.GetAvatar(PlayerCharactersManager.GetOpponentCurrentCharacterId());
    }
    
    public void SetCityScore(int player, int score)
    {
        cityScoreTextPlayers[player].text = score.ToString();
    }
    
    public void SetRoadScore(int player, int score)
    {
        tileScoreTextPlayers[player].text = score.ToString();
    }

    public void SetLocalPlayerScore(int localPlayerCityScore, int localPlayerTileScore)
    {
        LocalPlayerScore.text = (localPlayerCityScore + localPlayerTileScore).ToString();
    }
    
    public void SetRemotePlayerScore(int remotePlayerCityScore, int remotePlayerTileScore)
    {
        RemotePlayerScore.text = (remotePlayerCityScore + remotePlayerTileScore).ToString();
    }

    public void UpdateTime()
    {
        int currentCharacter = TerritoryWars.General.SessionManager.Instance.GetCurrentCharacter();
        players[currentCharacter].UpdateTimer();
        timeTextPlayers[currentCharacter].text = string.Format("{0:00}:{1:00}",
            Mathf.Floor(players[currentCharacter].time / 60),
            Mathf.Floor(players[currentCharacter].time % 60));
    }
    
    public void SetNames(string localPlayerName, string remotePlayerName)
    {
        LocalPlayerName.text = localPlayerName;
        RemotePlayerName.text = remotePlayerName;
    }



    public void UseJoker(int player)
    {
        if (players[player].jokerCount == 0)
            return;

        players[player].jokerCount--;
        players[player].jokersImage[players[player].jokerCount].color = JokerNotAvailableColor;
        players[player].jokerCountText.text = players[player].jokerCount.ToString();
    }

    public void SessionExit()
    {

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
