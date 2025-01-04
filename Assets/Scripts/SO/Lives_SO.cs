using UnityEngine;

[CreateAssetMenu]
public class Lives_SO : ScriptableObject
{
    public int maxLives = 6;
    public int livesRemaining;

    public void ResetData()
    {
        livesRemaining = maxLives;
    }
}
