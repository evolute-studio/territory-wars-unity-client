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
            _resultPopupComponents.Player1Name.text = player1Name;
            _resultPopupComponents.Player2Name.text = player2Name;
        }

        public void SetPlayersAvatars(Sprite localPlayerSprite, Sprite remotePlayerSprite)
        {
            _resultPopupComponents.Player1Avatar.sprite = localPlayerSprite;
            _resultPopupComponents.Player2Avatar.sprite = remotePlayerSprite;
        }

        public void SetPlayersScore(int player1Score, int player2Score)
        {
            _player1Score = player1Score;
            _player2Score = player2Score;
            
            _resultPopupComponents.Player1Score.text = player1Score.ToString();
            _resultPopupComponents.Player2Score.text = player2Score.ToString();
        }
        
        public void SetPlayersCityScores(int player1CityScores, int player2CityScores)
        {
            _resultPopupComponents.Player1CityScores.text = player1CityScores.ToString();
            _resultPopupComponents.CityEvoluteScoreTextPlayer1.text = $" x {player1CityScores}";
            _resultPopupComponents.Player2CityScores.text = player2CityScores.ToString();
            _resultPopupComponents.CityEvoluteScoreTextPlayer2.text = $" x {player2CityScores}";
        }
        
        public void SetPlayersCartScores(int player1CartScores, int player2CartScores)
        {
            _resultPopupComponents.Player1CartScores.text = player1CartScores.ToString();
            _resultPopupComponents.CartEvoluteScoreTextPlayer1.text = $" x {player1CartScores}";
            _resultPopupComponents.Player2CartScores.text = player2CartScores.ToString();
            _resultPopupComponents.CartEvoluteScoreTextPlayer2.text = $" x {player2CartScores}";
        }

        public void SetPlayerHeroAnimator(RuntimeAnimatorController localPlayerAnimator, RuntimeAnimatorController remotePlayerAnimator)
        {
            _resultPopupComponents.Player1Animator.runtimeAnimatorController = localPlayerAnimator;
            _resultPopupComponents.Player2Animator.runtimeAnimatorController = remotePlayerAnimator;
        }

        public void SetWinnerText(string winnerText)
        {
            _resultPopupComponents.WinnerText.text = winnerText;
        }
        
        public void SetPlayersJoker(int player1JokersCount, int player2JokersCount)
        {
            _player1JokersCount = player1JokersCount;
            _player2JokersCount = player2JokersCount;
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