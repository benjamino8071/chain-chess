using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Levels_SO : ScriptableObject
{
    public int levelOnLoad;

    public List<SectionData> sections;
    public Level finalLevel;

    public int finalLevelStarsRequirement;
    
    public List<Level> sectionOne;
    public List<Level> sectionTwo;
    public List<Level> sectionThree;
    public List<Level> sectionFour;
    public List<Level> sectionFive;
    public List<Level> sectionSix;
    public List<Level> sectionSeven;
    public List<Level> sectionEight;
    
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
        finalLevel.level = levelNumber;
    }

    public SectionData GetSection(int section)
    {
        foreach (SectionData sectionData in sections)
        {
            if (sectionData.section == section)
            {
                return sectionData;
            }
        }

        return default;
    }
    
    public int GetSectionStarsRequirement(int section)
    {
        foreach (SectionData sectionData in sections)
        {
            if (sectionData.section == section)
            {
                return sectionData.starsRequiredToUnlock;
            }
        }

        return -1;
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

        foreach (SectionData section in sections)
        {
            levels = levels.Concat(section.levels).ToList();
        }
        levels.Add(finalLevel);

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

[Serializable]
public struct SectionData
{
    public int section;
    public int starsRequiredToUnlock;
    public List<Level> levels;
}

[Serializable]
public struct SectionStarsRequirement
{
    public int section;
    public int stars;
}
