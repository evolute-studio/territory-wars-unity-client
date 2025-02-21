using Dojo.Starknet;
using UnityEngine;

public class Character : MonoBehaviour
{
    private Animator _animator;
    private CharacterAnimator _characterAnimator;
    public FieldElement Address;
    public int LocalId;
    public PlayerSide Side;
    
    public void Initialize(FieldElement address,PlayerSide side)
    {
        Address = address;
        Side = side;
        LocalId = side switch
        {
            PlayerSide.Blue => 0,
            PlayerSide.Red => 1,
            _ => 0
        };

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
    
    
}
