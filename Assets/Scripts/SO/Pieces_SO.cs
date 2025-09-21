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

    public Sprite GetSprite(Piece piece)
    {
        switch (piece)
        {
            case Piece.Pawn:
                return pawn;
            case Piece.Rook:
                return rook;
            case Piece.Knight:
                return knight;
            case Piece.Bishop:
                return bishop;
            case Piece.Queen:
                return queen;
            case Piece.King:
                return king;
        }

        return null;
    }

    public Material GetMaterial(PieceAbility piece)
    {
        switch (piece)
        {
            case PieceAbility.MustMove:
                return mustMoveMat;
            case PieceAbility.CaptureLover:
                return captureLoverMat;
            case PieceAbility.AlwaysMove:
                return alwaysMoveMat;
            case PieceAbility.Resetter:
                return resetterMat;
            case PieceAbility.Multiplier:
                return multiplierMat;
            case PieceAbility.StopTurn:
                return stopTurnMat;
        }

        return null;
    }
    
    public Color GetColour(PieceAbility piece, PieceColour pieceColour)
    {
        switch (piece)
        {
            case PieceAbility.None:
                return pieceColour == PieceColour.White ? whiteColor : blackColor;
            case PieceAbility.MustMove:
                return mustMoveColor;
            case PieceAbility.CaptureLover:
                return captureLoverColor;
            case PieceAbility.AlwaysMove:
                return alwaysMoveColor;
            case PieceAbility.Resetter:
                return resetterColor;
            case PieceAbility.Multiplier:
                return multiplierColor;
            case PieceAbility.StopTurn:
                return stopTurnColor;
        }

        return blackColor;
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
    StopTurn = 9
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
