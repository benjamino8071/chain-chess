using System;
using UnityEngine;

[CreateAssetMenu]
public class Settings_SO : ScriptableObject
{
    public float aspectRatioToChangeValue;
    public float absoluteMinimumAspectRatio;

    public int defaultWidth;
    public int defaultHeight;
    
    public UIModeSettings landscapeModeSettings;
    public UIModeSettings portraitModeSettings;
}

[Serializable]
public struct UIModeSettings
{
    public float cameraSize;
}
