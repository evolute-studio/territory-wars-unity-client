using System;
using System.Collections.Generic;
using TerritoryWars.General;
using TerritoryWars.Tile;
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

    public List<Image> imagePlayers;

    [Header("Players")]
    public List<PlayerInfo> players;

    public void Start() => Initialization();

    public void Initialization()
    {
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

        _board.OnTilePlaced += AddScore;
    }

    private void AddScore(TileData tile, int x, int y)
    {
        string config = tile.GetConfigWithoutRotation();

        int currentCharacter = TerritoryWars.General.SessionManager.Instance.GetCurrentCharacter();

        foreach (var side in config)
        {
            if (side == 'C')
            {
                cityScoreTextPlayers[currentCharacter].text = (int.Parse(cityScoreTextPlayers[currentCharacter].text) + 1).ToString();
            }
            else if (side == 'R')
            {
                tileScoreTextPlayers[currentCharacter].text = (int.Parse(tileScoreTextPlayers[currentCharacter].text) + 1).ToString();
            }
        }
    }

    public void UpdateTime()
    {
        int currentCharacter = TerritoryWars.General.SessionManager.Instance.GetCurrentCharacter();
        players[currentCharacter].UpdateTimer();
        timeTextPlayers[currentCharacter].text = string.Format("{0:00}:{1:00}",
            Mathf.Floor(players[currentCharacter].time / 60),
            Mathf.Floor(players[currentCharacter].time % 60));
    }



    public void UseJoker(int player)
    {
        if (players[player].jokerCount == 0)
            return;

        players[player].jokerCount--;
        players[player].jokersImage[players[player].jokerCount].color = JokerNotAvailableColor;
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
