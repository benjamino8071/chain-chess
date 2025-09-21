using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SaveData_SO : ScriptableObject
{
    public List<LevelSaveData> levelsSaveData;
}

[Serializable]
public struct LevelSaveData
{
    public int level;
    public int score;
}
