using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class ScaleTween : MonoBehaviour
{
    public float phaseInTime;
    public float phaseOutTime;

    public Vector3 selectedScale = Vector3.one;
    public Vector3 normalScale = Vector3.one;

    [Button]
    public void Selected()
    {
        LeanTween.scale(gameObject, selectedScale, phaseInTime).setOnComplete(BackToNormal);
    }

    private void BackToNormal()
    {
        LeanTween.scale(gameObject, normalScale, phaseInTime);
    }
    
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
