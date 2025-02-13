using UnityEditor.Animations;
using UnityEngine;

public class CharacterAnimator
{
    private Animator _animator;
    public AnimationClip IdleAnimation;
    public AnimationClip WinAnimation;
    public AnimationClip LoseAnimation;
    public AnimationClip HitAnimation;
    public AnimationClip CastAnimation;

    public CharacterAnimator(Animator animator)
    {
        _animator = animator;
    }
    
    public void Idle()
    {
        
    }
    
    public void Win()
    {
        _animator.SetTrigger("Win");
    }
    
    public void Lose()
    {
        _animator.SetTrigger("Lose");
    }
    
    public void Hit()
    {
        _animator.SetTrigger("Hit");
    }
    
    public void Cast()
    {
        _animator.SetTrigger("Cast");
    }
}
