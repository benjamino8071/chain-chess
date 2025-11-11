using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Levels_SO : ScriptableObject
{
    public int totalStars;
    
    public List<SectionData> sections;
    
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
    
    public Level GetLevel(int sectionNumber, int levelNumber)
    {
        foreach (SectionData section in sections)
        {
            if (section.section != sectionNumber)
            {
                continue;
            }
            
            foreach (Level level in section.levels)
            {
                if (level.level == levelNumber)
                {
                    return level;
                }
            }
        }
        
        return default;
    }

    [Button]
    public void CalculateTotalStars()
    {
        int starCount = 0;
        foreach (SectionData section in sections)
        {
            foreach (Level level in section.levels)
            {
                starCount += 3;
            }
        }
        
        totalStars = starCount;
    }
    
    [Button]
    public void SetSectionAndLevelNumbers()
    {
        for (int i = 0; i < sections.Count; i++)
        {
            SectionData section = sections[i];
            section.section = i + 1;
            
            for (int j = 0; j < sections[i].levels.Count; j++)
            {
                Level level = section.levels[j];

                level.section = section.section;
                level.level = j + 1;

                level.isLastInSection = j == sections[i].levels.Count - 1;
                
                section.levels[j] = level;
            }

            sections[i] = section;
        }
    }
}

[Serializable]
public struct Level
{
    public int section;
    public int level;
    public bool isLastInSection;
    public int star1Score;
    public int star2Score;
    public int star3Score;
    
    public List<StartingPieceSpawnData> positions;
}

[Serializable]
public struct SectionData
{
    public int section;
    public int starsRequiredToUnlock;
    public List<Level> levels;
}
