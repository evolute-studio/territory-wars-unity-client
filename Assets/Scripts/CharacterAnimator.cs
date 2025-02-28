using UnityEngine;

public class CharacterAnimator
{
    private static readonly int Win = Animator.StringToHash("Win");
    private static readonly int Lose = Animator.StringToHash("Lose");
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int Cast = Animator.StringToHash("Cast");
    private Animator _animator;

    public CharacterAnimator(Animator animator)
    {
        _animator = animator;
    }
    
    public void PlayIdle()
    {
        
    }
    
    public void PlayWin(bool value)
    {
        _animator.SetBool(Win, value);
    }
    
    public void PlayLose(bool value)
    {
        _animator.SetBool(Lose, value);
    }
    
    public void PlayHit()
    {
        _animator.SetTrigger(Hit);
    }
    
    public void PlayCast(bool value)
    {
        _animator.SetBool(Cast, value);
    }
}
