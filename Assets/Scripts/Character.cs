using System;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Serialization;

public class Character : MonoBehaviour
{
    private Animator _animator;
    private CharacterAnimator _characterAnimator;
    private int _id;
    
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
    
    
}
