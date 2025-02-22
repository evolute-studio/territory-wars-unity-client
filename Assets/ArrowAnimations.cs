using System;
using TerritoryWars.Tools;
using UnityEngine;

public class ArrowAnimations : MonoBehaviour
{
    public GameObject[] arrows;

    public void PlayRotationAnimation()
    {
        foreach (var arrow in arrows)
        {
            arrow.SetActive(true);
            arrow.GetComponent<SpriteAnimator>().Play();
        }
        
        
    }
    
    public void SetActiveArrow(bool isActive)
    {
        foreach (var arrow in arrows)
        {
            arrow.SetActive(isActive);
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
