using System;
using UnityEngine;

[CreateAssetMenu]
public class Settings_SO : ScriptableObject
{
    public float aspectRatioToChangeValue;
    public float absoluteMinimumAspectRatio;
    
    public UIModeSettings landscapeModeSettings;
    public UIModeSettings portraitModeSettings;

    public string saveDataKey = "saveDataSo";
}

[Serializable]
public struct UIModeSettings
{
    public float cameraSize;
}
