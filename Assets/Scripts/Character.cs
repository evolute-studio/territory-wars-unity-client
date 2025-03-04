using Dojo.Starknet;
using UnityEngine;

public class Character : MonoBehaviour
{
    private Animator _animator;
    private CharacterAnimator _characterAnimator;
    public FieldElement Address;
    public int LocalId;
    public PlayerSide Side;
    public int JokerCount;
    
    public void Initialize(FieldElement address,PlayerSide side, int jokerCount)
    {
        Address = address;
        Side = side;
        LocalId = side switch
        {
            PlayerSide.Blue => 0,
            PlayerSide.Red => 1,
            _ => 0
        };
        JokerCount = jokerCount;
        _animator = GetComponent<Animator>();
        _characterAnimator = new CharacterAnimator(_animator);
    }

    public void UpdateData(int jokerCount)
    {
        JokerCount = jokerCount;
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
