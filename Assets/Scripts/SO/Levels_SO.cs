using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Levels_SO : ScriptableObject
{
    public int levelOnLoad;

    public List<Level> sectionOne;
    public List<Level> sectionTwo;
    public List<Level> sectionThree;
    public List<Level> sectionFour;
    public List<Level> sectionFive;
    public List<Level> sectionSix;
    public List<Level> sectionSeven;
    public List<Level> sectionEight;
    public List<Level> sectionNine;
    public List<Level> sectionTen;

    [Title("Level Numbers")]
    [Button]
    public void SetAllLevelNumbers()
    {
        int levelNumber = 1;

        levelNumber = SetLevelNumber(sectionOne, levelNumber);
        levelNumber = SetLevelNumber(sectionTwo, levelNumber);
        levelNumber = SetLevelNumber(sectionThree, levelNumber);
        levelNumber = SetLevelNumber(sectionFour, levelNumber);
        levelNumber = SetLevelNumber(sectionFive, levelNumber);
        levelNumber = SetLevelNumber(sectionSix, levelNumber);
        levelNumber = SetLevelNumber(sectionSeven, levelNumber);
        levelNumber = SetLevelNumber(sectionEight, levelNumber);
        levelNumber = SetLevelNumber(sectionNine, levelNumber);
        SetLevelNumber(sectionTen, levelNumber);
    }

    private int SetLevelNumber(List<Level> section, int levelNumber)
    {
        for (int i = 0; i < section.Count; i++)
        {
            Level level = section[i];
            level.level = levelNumber;
            section[i] = level;
            
            levelNumber++;
        }

        return levelNumber;
    }
    
    [Title("Reserved Levels")]
    public List<Level> reservedLevels;

    public List<Level> AllLevels()
    {
        List<Level> levels = sectionOne;
        levels = levels.Concat(sectionTwo)
            .Concat(sectionThree)
            .Concat(sectionFour)
            .Concat(sectionFive)
            .Concat(sectionSix)
            .Concat(sectionSeven)
            .Concat(sectionEight)
            .Concat(sectionNine)
            .Concat(sectionTen).ToList();

        return levels;
    }
    
    public Level GetLevelOnLoad()
    {
        List<Level> allLevels = AllLevels();
        for (int i = 0; i < allLevels.Count; i++)
        {
            if (i + 1 == levelOnLoad)
            {
                Level level = allLevels[i];
                return level;
            }
        }
        
        return default;
    }
}
