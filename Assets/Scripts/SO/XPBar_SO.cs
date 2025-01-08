using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class XPBar_SO : ScriptableObject
{
    public Dictionary<Piece, float> capturePieceValue = new()
    {
        {Piece.Pawn, 1},
        {Piece.Bishop, 3},
        {Piece.Knight, 3},
        {Piece.Rook, 5},
        {Piece.Queen, 9},
        {Piece.King, 9}
    };
}
