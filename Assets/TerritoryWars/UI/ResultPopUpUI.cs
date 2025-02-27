using System;
using DG.Tweening;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class ResultPopUpUI : MonoBehaviour
    {
        [SerializeField] private GameObject _resultPopup;
        [SerializeField] private ResultPopupComponents _resultPopupComponents;
        
        [SerializeField] private float _animationDuration = 0.5f;

        private bool IsPlayer1Winner => _player1Score > _player2Score;
        private bool IsDraw => _player1Score == _player2Score;
        
        private int _player1CityScores;
        private int _player1CartScores;
        private int _player1JokersCount;
        private int _player1Score;
        
        private int _player2CityScores;
        private int _player2CartScores;
        private int _player2JokersCount;
        private int _player2Score;
        
        public string WonText = "You won!";
        public string LoseText = "You lose!";
        public string DrawText = "Draw!";

        public void SetupButtons()
        {
            _resultPopupComponents.FinishButton.onClick.AddListener(() => CustomSceneManager.Instance.LoadLobby(0));
        }
        
        public void SetResultPopupActive(bool active)
        {
            _resultPopup.SetActive(active);
        }

        public void SetPlayersName(string player1Name, string player2Name)
        {
            string[] playerNames = SetLocalPlayerData.GetLocalPlayerString(player1Name, player2Name);
            _resultPopupComponents.Player1Name.text = playerNames[0];
            _resultPopupComponents.Player2Name.text = playerNames[1];
        }

        public void SetPlayersAvatars(Sprite localPlayerSprite, Sprite remotePlayerSprite)
        {
            Sprite[] playerSprites = SetLocalPlayerData.GetLocalPlayerSprite(localPlayerSprite, remotePlayerSprite);
            _resultPopupComponents.Player1Avatar.sprite = playerSprites[0];
            _resultPopupComponents.Player2Avatar.sprite = playerSprites[1];
        }

        public void SetPlayersScore(int player1Score, int player2Score)
        {
            int[] playerScores = SetLocalPlayerData.GetLocalPlayerInt(player1Score, player2Score);
            _player1Score = playerScores[0];
            _player2Score = playerScores[1];
            
            _resultPopupComponents.Player1Score.text = playerScores[0].ToString();
            _resultPopupComponents.Player2Score.text = playerScores[1].ToString();
        }
        
        public void SetPlayersCityScores(int player1CityScores, int player2CityScores)
        {
            int[] playersCityScores = SetLocalPlayerData.GetLocalPlayerInt(player1CityScores, player2CityScores);
            
            _resultPopupComponents.Player1CityScores.text = playersCityScores[0].ToString();
            _resultPopupComponents.CityEvoluteScoreTextPlayer1.text = $" x {playersCityScores[0]}";
            _resultPopupComponents.Player2CityScores.text = playersCityScores[1].ToString();
            _resultPopupComponents.CityEvoluteScoreTextPlayer2.text = $" x {playersCityScores[1]}";
        }
        
        public void SetPlayersCartScores(int player1CartScores, int player2CartScores)
        {
            int[] playersCartScores = SetLocalPlayerData.GetLocalPlayerInt(player1CartScores, player2CartScores);
            _resultPopupComponents.Player1CartScores.text = playersCartScores[0].ToString();
            _resultPopupComponents.CartEvoluteScoreTextPlayer1.text = $" x {playersCartScores[0]}";
            _resultPopupComponents.Player2CartScores.text = playersCartScores[1].ToString();
            _resultPopupComponents.CartEvoluteScoreTextPlayer2.text = $" x {playersCartScores[1]}";
        }

        public void SetPlayerHeroAnimator(RuntimeAnimatorController localPlayerAnimator, RuntimeAnimatorController remotePlayerAnimator)
        {
            RuntimeAnimatorController[] playersAnimatorController = SetLocalPlayerData.GetLocalPlayerAnimator(localPlayerAnimator, remotePlayerAnimator);
            _resultPopupComponents.Player1Animator.runtimeAnimatorController = playersAnimatorController[0];
            _resultPopupComponents.Player2Animator.runtimeAnimatorController = playersAnimatorController[1];
        }

        public void SetWinnerText(string winnerText)
        {
            _resultPopupComponents.WinnerText.text = winnerText;
        }
        
        public void SetPlayersJoker(int player1JokersCount, int player2JokersCount)
        {
            int[] playersJokersCount = SetLocalPlayerData.GetLocalPlayerInt(player1JokersCount, player2JokersCount);
            _player1JokersCount = playersJokersCount[0];
            _player2JokersCount = playersJokersCount[1];
        }

        public void ViewResults()
        {
            for(int i = 0; i < _player1JokersCount; i++)
            {
                _resultPopupComponents.Player1Jokers[i].GetComponent<CanvasGroup>().alpha = 1f;
            }
            
            for(int i = 0; i < _player2JokersCount; i++)
            {
                _resultPopupComponents.Player2Jokers[i].GetComponent<CanvasGroup>().alpha = 1f;
            }
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_resultPopupComponents.CityEvoluteScoreGOPlayer1.GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));
            sequence.Append(_resultPopupComponents.CartEvoluteScoreGOPlayer1.GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));

            if (_player1JokersCount > 0)
            {
                for (int i = 0; i < _player1JokersCount; i++)
                {
                    sequence.Append(_resultPopupComponents.JokerEvoluteScoreGOPlayer1[i].GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));
                }
            }
            
            sequence.Append(_resultPopupComponents.CityEvoluteScoreGOPlayer2.GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));
            sequence.Append(_resultPopupComponents.CartEvoluteScoreGOPlayer2.GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));
            
            if (_player2JokersCount > 0)
            {
                for (int i = 0; i < _player2JokersCount; i++)
                {
                    sequence.Append(_resultPopupComponents.JokerEvoluteScoreGOPlayer2[i].GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));
                }
            }
            
            sequence.Append(_resultPopupComponents.Player1ScoreGO.GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));
            sequence.Append(_resultPopupComponents.Player2ScoreGO.GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));
            
            sequence.Append(_resultPopupComponents.WinnerGO.GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));
            
            sequence.Append(_resultPopupComponents.Player1HeroSpriteRenderer.DOFade(1f, _animationDuration).OnComplete(
                () =>
                {
                    if (IsPlayer1Winner)
                    {
                        _resultPopupComponents.Player1Animator.SetBool("Win", true);
                    }
                    else if(!IsPlayer1Winner)
                    {
                        _resultPopupComponents.Player1Animator.SetBool("Lose", true);
                    }
                    else if(IsDraw)
                    {
                        _resultPopupComponents.Player1Animator.SetBool("Lose", true);
                    }
                    
                }));
            sequence.Append(_resultPopupComponents.Player2HeroSpriteRenderer.DOFade(1f, _animationDuration).OnComplete(
                () =>
                {
                    if (IsPlayer1Winner)
                    {
                        _resultPopupComponents.Player2Animator.SetBool("Lose", true);
                    }
                    else if(!IsPlayer1Winner)
                    {
                        _resultPopupComponents.Player2Animator.SetBool("Win", true);
                    }
                    else if (IsDraw)
                    {
                        _resultPopupComponents.Player2Animator.SetBool("Lose", true);
                    }
                }));
            sequence.Append(_resultPopupComponents.Buttons.GetComponent<CanvasGroup>().DOFade(1f, _animationDuration));

            sequence.Play();

        }

        public void TestInit()
        {
            _player1JokersCount = 2;
            _player2JokersCount = 3;
        }

    }
}