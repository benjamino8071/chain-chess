using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class ScaleTween : MonoBehaviour
{
    public float phaseInTime;
    public float phaseOutTime;

    [Button]
    public void PhaseIn()
    {
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, phaseInTime);
    }
    
    [Button]
    public void PhaseOut()
    {
        transform.localScale = Vector3.one;
        LeanTween.scale(gameObject, Vector3.zero, phaseOutTime);
    }
}
