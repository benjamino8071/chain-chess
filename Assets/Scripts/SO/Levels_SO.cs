using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Levels_SO : ScriptableObject
{
    public int levelOnLoad;
    
    public List<Level> levelsData;

    public Level GetLevelOnLoad()
    {
        foreach (Level level in levelsData)
        {
            if (level.number == levelOnLoad)
            {
                return level;
            }
        }
        
        return default;
    }
}
