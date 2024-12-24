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
    
    public Dictionary<Piece, float> timeCost = new()
    {
        {Piece.Bishop, 3},
        {Piece.Knight, 3},
        {Piece.Rook, 5},
        {Piece.Queen, 9},
    };
    
    public readonly Dictionary<UpgradeTypes, int> upgradesCost = new()
    {
        { UpgradeTypes.ReducePromotionCost, 30 },
        { UpgradeTypes.IncreaseMultiplierAmount, 21 },
        { UpgradeTypes.IncreaseBaseAmountGained , 18},
        { UpgradeTypes.ReduceRespawnCost, 40}
    };

    public readonly Dictionary<ArtefactTypes, int> artefactsCost = new()
    {
        { ArtefactTypes.EnemyLineOfSight, 6},
        { ArtefactTypes.DestroyChainStayAlive, 30},
        { ArtefactTypes.UseCapturedPieceStraightAway, 15},
        { ArtefactTypes.CaptureKingClearRoom, 40}
    };

    public float sellArtefactDivider = 2;
    
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

        timeCost[Piece.Bishop] = 3;
        timeCost[Piece.Knight] = 3;
        timeCost[Piece.Rook] = 5;
        timeCost[Piece.Queen] = 9;
        Debug.Log("Timer_SO cache reset");
    }
}
