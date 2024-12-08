using UnityEngine;

[CreateAssetMenu]
public class Timer_SO : ScriptableObject
{
    public float maxTime;

    public float contFromRoomPenalty = 10f;

    public float levelCompleteBonus = 100f;
    
    public float currentTime;
}
