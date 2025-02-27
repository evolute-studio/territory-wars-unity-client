using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TerritoryWars.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars
{
    public class ClashAnimation : MonoBehaviour
    {
        public SpriteAnimator SwordsAnimator;
        public SpriteAnimator FlagsAnimator;
        public TextMeshPro PointsText;
        public SpriteRenderer BackgroundCircle;
        
        private Queue<Action> _animationQueue = new Queue<Action>();
        private List<List<Sprite>> FlagsAnimations;
        
        public List<Sprite> SwordsAnimations;
        public List<Sprite> FirstPlayerFlagsAnimations;
        public List<Sprite> SecondPlayerFlagsAnimations;
        public List<Sprite> MixedFlagsAnimations;
        
        public Color[] PlayerColors;
        
        
        
        // first action dependencies
        
        // second action dependencies
        private int WinPlayerId;
        
        // third action dependencies
        private Action Recoloring;
        
        // fourth action dependencies
        private ushort[] Points;
        
        public void Initialize(Vector3 position, int winPlayerId, ushort[] points, Action recoloring)
        {
            transform.position = position;
            WinPlayerId = winPlayerId;
            Points = points;
            Recoloring = recoloring;
            
            FlagsAnimations = new List<List<Sprite>>()
            {
                FirstPlayerFlagsAnimations,
                SecondPlayerFlagsAnimations,
                MixedFlagsAnimations
            };
            
            _animationQueue.Enqueue(FirstAction);
            _animationQueue.Enqueue(SecondAction);
            _animationQueue.Enqueue(ThirdAction);
            _animationQueue.Enqueue(FourthAction);
            _animationQueue.Enqueue(FifthAction);
            
            
            NextAction();
        }
        
        private void FirstAction()
        {
            SwordsAnimator.Play(SwordsAnimations.ToArray());
            SwordsAnimator.OnAnimationEnd = NextAction;
        }
        
        private void SecondAction()
        {
            if (Points[0] == Points[1])
            {
                WinPlayerId = 2;
            }
            BackgroundCircle.color = PlayerColors[WinPlayerId];
            FlagsAnimator.Play(FlagsAnimations[WinPlayerId].ToArray());
            FlagsAnimator.OnAnimationEnd = NextAction;
            
            BackgroundCircle.gameObject.SetActive(true);
            PointsText.gameObject.SetActive(true);
            PointsText.text = (Points[0] + Points[1]).ToString();
            
            PointsText.color = new Color(1, 1, 1, 0);
            BackgroundCircle.color = new Color(PlayerColors[WinPlayerId].r, PlayerColors[WinPlayerId].g, PlayerColors[WinPlayerId].b, 0);
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(PointsText.DOFade(1, 0.2f));
            sequence.Join(BackgroundCircle.DOFade(1, 0.2f));
        }
        
        private void ThirdAction()
        {
            Recoloring.Invoke();
            NextAction();
        }
        
        private void FourthAction()
        {
            Invoke(nameof(NextAction), 1.5f);
        }

        private void FifthAction()
        {
            List<Sprite> mirrorAnimation = FlagsAnimations[WinPlayerId].ToList();
            mirrorAnimation.Reverse();
            
            FlagsAnimator.Play(mirrorAnimation.ToArray());
            
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.2f);
            sequence.Append(FlagsAnimator.GetComponent<SpriteRenderer>().DOFade(0, 0.2f));
            sequence.Join(SwordsAnimator.GetComponent<SpriteRenderer>().DOFade(0, 0.2f));
            sequence.Join(PointsText.DOFade(0, 0.2f));
            sequence.Join(BackgroundCircle.DOFade(0, 0.2f));
            sequence.AppendCallback(NextAction);
        }

        private void NextAction()
        {
            if (_animationQueue.Count > 0)
            {
                _animationQueue.Dequeue().Invoke();
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }
}