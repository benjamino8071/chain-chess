using System;
using UnityEngine;

[CreateAssetMenu]
public class Settings_SO : ScriptableObject
{
    public float aspectRatioToChangeValue;
    
    public UIModeSettings landscapeModeSettings;
    public UIModeSettings portraitModeSettings;
}

[Serializable]
public struct UIModeSettings
{
    public float cameraSize;
}
