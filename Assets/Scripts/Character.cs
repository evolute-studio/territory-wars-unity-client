using System;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Serialization;

public class Character : MonoBehaviour
{
    private Animator _animator;
    public bool IsMirror = false;
    private int _id;
    
    public void Start() => Initialize();
    
    public void Initialize()
    {
        _animator = GetComponent<Animator>();
    }
    
    
}
