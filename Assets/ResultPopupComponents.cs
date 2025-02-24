using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultPopupComponents : MonoBehaviour
{
    [Header("Images")]
    public Image Player1Avatar;
    public Image Player2Avatar;

    [Header("Player1 Texts")]
    public TextMeshProUGUI Player1Name;
    public TextMeshProUGUI Player1Score;
    public TextMeshProUGUI Player1CityScores;
    public TextMeshProUGUI Player1CartScores;
    
    [Header("Player2 Texts")]
    public TextMeshProUGUI Player2Name;
    public TextMeshProUGUI Player2Score;
    public TextMeshProUGUI Player2CityScores;
    public TextMeshProUGUI Player2CartScores;
    
    [Header("Winner Text")]
    public GameObject WinnerGO;
    public TextMeshProUGUI WinnerText;
    
    [Header("GameObjects")]
    public List<GameObject> Player1Jokers;
    public List<GameObject> Player2Jokers;
    public GameObject Player1ScoreGO;
    public GameObject Player2ScoreGO;

    [Header("Buttons")]
    public GameObject Buttons;
    public Button FinishButton;
    public Button ToBoardButton;
    
    [Header("EvoluteScoreText")]
    public TextMeshProUGUI CityEvoluteScoreTextPlayer1;
    public TextMeshProUGUI CartEvoluteScoreTextPlayer1;
    public TextMeshProUGUI CityEvoluteScoreTextPlayer2;
    public TextMeshProUGUI CartEvoluteScoreTextPlayer2;
    
    [Header("EvoluteScore")]
    public GameObject CityEvoluteScoreGOPlayer1;
    public GameObject CartEvoluteScoreGOPlayer1;
    public GameObject CityEvoluteScoreGOPlayer2;
    public GameObject CartEvoluteScoreGOPlayer2;
    public GameObject[] JokerEvoluteScoreGOPlayer1;
    public GameObject[] JokerEvoluteScoreGOPlayer2;

    [Header("Players Hero")]
    public Animator Player1Animator;
    public Animator Player2Animator;
    public SpriteRenderer Player1HeroSpriteRenderer;
    public SpriteRenderer Player2HeroSpriteRenderer;
}
