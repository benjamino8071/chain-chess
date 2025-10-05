using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SaveData_SO : ScriptableObject
{
    public List<LevelSaveData> levels;

    public int sectionLastLoaded;
    public int levelLastLoaded;

    public bool isFirstTime;
    
    public BoardVariant boardVariant;

    public bool audio;

    public int windowWidth;
    public int windowHeight;
    
    public bool isFullscreen;
}

[Serializable]
public struct LevelSaveData
{
    public int section;
    public int level;
    
    public int score;
    public int starsScored;
}
