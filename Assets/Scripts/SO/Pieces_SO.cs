using System;
using System.Collections.Generic;
using UnityEngine;
using SystemRandom = System.Random;

[CreateAssetMenu]
public class Pieces_SO : ScriptableObject
{
    public float pieceSpeed;
    public float alwaysMoveSpeed;
    
    public Sprite pawn;
    public Sprite rook;
    public Sprite knight;
    public Sprite bishop;
    public Sprite queen;
    public Sprite king;

    public Color whiteColor;
    public Color blackColor;
    public Color resetterColor;
    public Color mustMoveColor;
    public Color multiplierColor;
    public Color captureLoverColor;
    public Color stopTurnColor;
    public Color alwaysMoveColor;
    
    [Header("AI-Specific")]
    public Material noneMat;
    public Material resetterMat;
    public Material mustMoveMat;
    public Material multiplierMat;
    public Material captureLoverMat;
    public Material stopTurnMat;
    public Material alwaysMoveMat;

    public float aiThinkingTime;
    public float alwaysMoveThinkingTime;

    public string GetPieceAbilityText(PieceAbility pieceAbility)
    {
        switch(pieceAbility)
        {
            case PieceAbility.None:
                {
                    return "Normal";
                }
            case PieceAbility.Resetter:
                {
                    return "Resetter";
                }
            case PieceAbility.MustMove:
                {
                    return "Turn Mover";
                }
            case PieceAbility.Multiplier:
                {
                    return "Multiplier";
                }
            case PieceAbility.CaptureLover:
                {
                    return "Pouncer";
                }
            case PieceAbility.StopTurn:
                {
                    return "Turn Stopper";
                }
            case PieceAbility.AlwaysMove:
                {
                    return "Move Mover";
                }
        }

        Debug.Log("Invalid piece ability. We should not get here!");
        return "";
    }
}

public enum Piece
{
    NotChosen,
    Pawn,
    Rook,
    Knight,
    Bishop,
    Queen,
    King
}

public enum PieceColour
{
    Black,
    White
}

public enum PieceAbility
{
    None = 0,
    MustMove = 1,
    CaptureLover = 3,
    AlwaysMove = 5,
    Resetter = 7,
    Multiplier = 8,
    StopTurn = 9,
    Foo = 10
}

public enum PieceState
{
    WaitingForTurn = 0,
    FindingMove = 1,
    Moving = 2,
    NotInUse = 3,
    Paused = 4,
    Blocked = 5,
    EndGame = 6
}

[Serializable]
public struct StartingPieceSpawnData
{
    public Piece piece;
    public PieceColour colour;
    public PieceAbility ability;
    public Vector2 position;
}
