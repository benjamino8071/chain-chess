using UnityEngine;

[CreateAssetMenu]
public class Board_SO : ScriptableObject
{
    [Range(0.55f, 0.9f)] public float maxY;
    [Range(0f, 0.30f)] public float minY;
}
