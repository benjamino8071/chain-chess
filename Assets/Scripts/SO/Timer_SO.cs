using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Timer_SO : ScriptableObject
{
    public float startingTime;
    
    public float currentTime;

    public float playerRespawnDivideCost = 2;
    
    public float timerMultiplier = 1;

    public float timerMultiplierMultiplier = 1.11f;

    public float timePenaltyOnReload = 0;

    public float showTimeChangeAmount = 3;

    public Dictionary<Piece, float> capturePieceTimeAdd = new()
    {
        {Piece.Pawn, 1},
        {Piece.Bishop, 3},
        {Piece.Knight, 3},
        {Piece.Rook, 5},
        {Piece.Queen, 9},
        {Piece.King, 9}
    };
    
    public Dictionary<Piece, float> capturePieceTimeRemove = new()
    {
        {Piece.Bishop, 3},
        {Piece.Knight, 3},
        {Piece.Rook, 5},
        {Piece.Queen, 9},
    };

    public void ResetData()
    {
        currentTime = startingTime;

        playerRespawnDivideCost = 2;

        timerMultiplier = 1;
        timerMultiplierMultiplier = 1.11f;
        
        timePenaltyOnReload = 0;
        
        capturePieceTimeAdd[Piece.Pawn] = 1;
        capturePieceTimeAdd[Piece.Bishop] = 3;
        capturePieceTimeAdd[Piece.Knight] = 3;
        capturePieceTimeAdd[Piece.Rook] = 5;
        capturePieceTimeAdd[Piece.Queen] = 9;
        capturePieceTimeAdd[Piece.King] = 9;

        capturePieceTimeRemove[Piece.Bishop] = 3;
        capturePieceTimeRemove[Piece.Knight] = 3;
        capturePieceTimeRemove[Piece.Rook] = 5;
        capturePieceTimeRemove[Piece.Queen] = 9;
        Debug.Log("Timer_SO cache reset");
    }
}
