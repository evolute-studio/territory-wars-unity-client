using TerritoryWars.Tools;
using TMPro;
using UnityEngine;

namespace TerritoryWars
{
    public class Locker : MonoBehaviour
    {
        public int cost = 100;
        public Sprite[] UnlockAnimation;
        public Sprite[] ShineAnimation;
        
        public SpriteAnimator Animator;
        public TextMeshProUGUI CostText;
        public Transform CostTransform;
        public NamePanelController PlayerData;
        
        private bool isUnlocked = false;
        
        public void Unlock()
        {
            if (isUnlocked)
                return;
            isUnlocked = true;
            PlayerData.ChangeEvoluteBalance(-cost);
            
            Animator.ChangeSprites(UnlockAnimation);
            Animator.OnAnimationEnd = () =>
            {
                Animator.ChangeSprites(ShineAnimation);
                Animator.OnAnimationEnd = null;
                Animator.OnAnimationEnd = () =>
                {
                    Animator.gameObject.SetActive(false);
                };
                Animator.Play();
                
            };
            Animator.Play();
            
            
            CostTransform.gameObject.SetActive(false);
        }
    }
}