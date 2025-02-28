using TerritoryWars.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars
{
    public class Locker : MonoBehaviour
    {
        public int cost = 100;
        public Sprite[] UnlockAnimation;
        public Sprite[] ShineAnimation;
        
        public SpriteAnimator Animator;
        public Image IconRenderer;
        public NamePanelController PlayerData;
        
        public bool isUnlocked = false;
        
        public void Unlock()
        {
            if (isUnlocked)
                return;
            isUnlocked = true;
            
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
        }
        
        public void FastUnlock()
        {
            if (isUnlocked)
                return;
            isUnlocked = true;
            Animator.gameObject.SetActive(false);
        }
    }
}