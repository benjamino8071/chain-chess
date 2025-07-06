using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Enemy_SO : ScriptableObject
{
    public float moveSpeed;
    
    public Sprite pawn;
    public Sprite rook;
    public Sprite knight;
    public Sprite bishop;
    public Sprite king;
    public Sprite queen;

    [Header("Piece Effect Type Materials")]
    public Material noneMat;
    public Material mustMoveMat;
    public Material glitchedMat;
    public Material captureLoverMat;

    //Key = spawn point, Value = (chosen piece, room number)
    public Dictionary<Vector3, (Piece, int, ElEnemyController.PieceEffectType)> cachedSpawnPoints = new();

    public void ResetData()
    {
        cachedSpawnPoints.Clear();
        Debug.Log("Enemy_SO cache reset");
    }
}
