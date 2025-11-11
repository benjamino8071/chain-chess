using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class ScaleTween : MonoBehaviour
{
    public float phaseInTime;
    public float phaseOutTime;

    public Vector3 normalScale = Vector3.one;
    
    [Button]
    public void Enlarge()
    {
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, normalScale, phaseInTime);
    }
    
    [Button]
    public void Shrink()
    {
        transform.localScale = normalScale;
        LeanTween.scale(gameObject, Vector3.zero, phaseOutTime);
    }
}
