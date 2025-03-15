using System.Linq;
using TerritoryWars.General;
using TerritoryWars.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TerritoryWars.UI
{
    public class SessionTimerUI : MonoBehaviour
    {
        [Header("Turn text info")]
        public TextMeshProUGUI TurnText;
        public string LocalPlayerTurnText = "Your turn now";
        [Tooltip(" @ - player name")]
        public string OpponentPlayerTurnText = $"Waiting for @ turn";
        
        [Header("Hourglass")]
        public SpriteAnimator SpriteAnimator;
        public Sprite[] IdleAnimationSprites;
        public float IdleAnimationDuration = 1f;
        public Sprite[] RotationAnimationSprites;
        public float RotationAnimationDuration = 0.5f;
        
        [Header("Timer")]
        public TextMeshProUGUI TimerText;
        public float TurnDuration = 120f;
        private float _currentTurnTime;
        
        [Header("Events")]
        public UnityEvent OnLocalPlayerTurnEnd;
        public UnityEvent OnOpponentPlayerTurnEnd;
        
        private bool _isLocalPlayerTurn => SessionManager.Instance.IsLocalPlayerTurn;
        private bool _isTimerActive => _currentTurnTime > 0;
        private string _opponentPlayerName => SessionManager.Instance.LocalPlayer.LocalId == 0 
            ? SessionManager.Instance.PlayersData[1].username 
            : SessionManager.Instance.PlayersData[0].username;

        public void Update()
        {
            UpdateTimer();
            UpdateTurnText();
        }

        public void StartTurnTimer()
        {
            RotateHourglass();
            _currentTurnTime = TurnDuration;
        }
        
        private void UpdateTimer()
        {
            if (!_isTimerActive) return;
            
            _currentTurnTime -= Time.deltaTime;
            TimerText.text = $"{Mathf.FloorToInt(_currentTurnTime / 60):00}:{Mathf.FloorToInt(_currentTurnTime % 60):00}";
            
            if (_currentTurnTime <= 0)
            {
                TimerText.text = "00:00";
                EndTimer();
            }
        }
        
        private void EndTimer()
        {
            CustomLogger.LogInfo("End timer");
            if (_isLocalPlayerTurn)
            {
                CustomLogger.LogInfo("Local player turn end");
                OnLocalPlayerTurnEnd?.Invoke();
            }
            else
            {
                CustomLogger.LogInfo("Opponent player turn end");
                OnOpponentPlayerTurnEnd?.Invoke();
            }
            
        }
        
        private void RotateHourglass()
        {
            SpriteAnimator.duration = RotationAnimationDuration;
            SpriteAnimator.Play(RotationAnimationSprites).OnComplete(
                () =>
                {
                    SpriteAnimator.duration = IdleAnimationDuration;
                    SpriteAnimator.Play(IdleAnimationSprites);
                });
        }
        
        private void UpdateTurnText()
        {
            string baseText;
            if (_isLocalPlayerTurn)
            {
                baseText = LocalPlayerTurnText;
            }
            else
            {
                baseText = OpponentPlayerTurnText.Replace("@", _opponentPlayerName);
            }
            
            int visibleDots = 3 - ((int)(_currentTurnTime % 3));
            string dots = string.Join("", new string[3].Select((_, index) => 
                $"<color=#{(index < visibleDots ? "FFFFFFFF" : "FFFFFF00")}>.</color>"));
            
            TurnText.text = baseText + dots;
        }

    }
}