using UnityEngine;

[CreateAssetMenu]
public class Board_SO : ScriptableObject
{
    [Range(0, 30)] public float maxY;
    [Range(-30, 0)] public float minY;

    public bool hideMainMenuTrigger;
}
