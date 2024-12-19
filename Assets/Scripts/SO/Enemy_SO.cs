using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Enemy_SO : ScriptableObject
{
    public Sprite pawn;
    public Sprite rook;
    public Sprite knight;
    public Sprite bishop;
    public Sprite king;
    public Sprite queen;

    [Header("Piece Effect Type Materials")]
    public Material noneMat;
    public Material glitchedMat;

    //Key = spawn point, Value = (chosen piece, room number)
    public Dictionary<Vector3, (Piece, int)> cachedSpawnPoints = new();

    [Button]
    public void ResetCachedSpawnPoints()
    {
        cachedSpawnPoints.Clear();
        Debug.Log("Cached enemy spawn points cleared");
    }
}
