using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class Chain_SO : ScriptableObject
{
    [FormerlySerializedAs("arrowPointingToNextPiecePrefab")] public Sprite arrowPointingToNextPiece;
}
