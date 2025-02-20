using System;
using TerritoryWars.Tools;
using UnityEngine;

public class ArrowAnimations : MonoBehaviour
{
    public GameObject[] arrows;

    private void Start()
    {
        arrows[0].GetComponent<SpriteAnimator>().OnAnimationEnd = OffArrows;
    }

    public void PlayRotationAnimation()
    {
        foreach (var arrow in arrows)
        {
            arrow.SetActive(true);
        }
        
        
    }

    public void OffArrows()
    {
        foreach (var arrow in arrows)
        {
            arrow.SetActive(false);
        }
    }
    
    
}
