using UnityEngine;

[CreateAssetMenu]
public class GridSystem_SO : ScriptableObject
{
    public Color baseColor, offsetColor;

    public int width, height;

    public int seed;
    public bool useSeedInputOnNextLoad;
}
