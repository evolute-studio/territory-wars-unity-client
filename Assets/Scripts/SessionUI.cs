using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SessionUI : MonoBehaviour
{
    public Color JokerNotAvailableColor;
    public Color JokerAvailableColor;
    public List<TextMeshProUGUI> cityScoreTextPlayers;
    public List<TextMeshProUGUI> tileScoreTextPlayers;
    public List<TextMeshProUGUI> timeTextPlayers;

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
    }

    public void UseJoker(int player)
    {
        if(players[player].jokerCount == 0)
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
    }
}
