using UnityEngine;

public class Character : MonoBehaviour
{
    private Animator _animator;
    private CharacterAnimator _characterAnimator;
    public int Id;
    
    public void Start() => Initialize();
    
    public void Initialize()
    {
        _animator = GetComponent<Animator>();
        _characterAnimator = new CharacterAnimator(_animator);
    }

    public void StartSelecting()
    {
        _characterAnimator.PlayCast(true);
    }
    
    public void EndTurn()
    {
        _characterAnimator.PlayCast(false);
        _characterAnimator.PlayHit();
    }
    
    public void SetAnimatorController(RuntimeAnimatorController controller)
    {
        _animator = GetComponent<Animator>();
        _animator.runtimeAnimatorController = controller;
    }
    
    
}
