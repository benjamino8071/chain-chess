using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Levels_SO : ScriptableObject
{
    public int levelOnLoad;
    
    public List<Level> levelsData;

    [Title("Reserved Levels")]
    public List<Level> reservedLevels;
    
    public Level GetLevelOnLoad()
    {
        foreach (Level level in levelsData)
        {
            if (levelsData.IndexOf(level)+1 == levelOnLoad)
            {
                return level;
            }
        }
        
        return default;
    }
}
