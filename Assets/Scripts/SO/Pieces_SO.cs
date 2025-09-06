using System;
using System.Collections.Generic;
using UnityEngine;
using SystemRandom = System.Random;

[CreateAssetMenu]
public class Pieces_SO : ScriptableObject
{
    public float pieceSpeed;
    
    public Sprite pawn;
    public Sprite rook;
    public Sprite knight;
    public Sprite bishop;
    public Sprite queen;
    public Sprite king;

    public Color whiteColor;
    public Color blackColor;
    public Color mustMoveColor;
    public Color captureLoverColor;
    public Color leaveBehindColor;
    public Color tileDestroyerColor;
    
    [Header("AI-Specific")]
    public Material noneMat;
    public Material mustMoveMat;
    public Material glitchedMat;
    public Material captureLoverMat;
    public Material leaveBehindMat;
    public Material tileDestroyerMat;

    public float aiThinkingTime;
    public float alwaysMoveThinkingTime;
}

public enum PieceColour
{
    Black,
    White
}

public enum PieceAbility
{
    None,
    MustMove,
    Glitched,
    CaptureLover,
    LeaveBehind,
    AlwaysMove,
    TileDestroyer,
    Resetter
}

[Serializable]
public struct Level
{
    public int level;
    public int turns;
    public int star1Score;
    public int star2Score;
    public int star3Score;
    
    public List<StartingPieceSpawnData> positions;
}

[Serializable]
public struct StartingPieceSpawnData
{
    public Piece piece;
    public PieceColour colour;
    public PieceAbility ability;
    public Vector2 position;
}
