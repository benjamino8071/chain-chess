using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

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
    TileDestroyer
}

[Serializable]
public struct Level
{
    public int level;
    public int turns;
    public int star1Score;
    public int star2Score;
    public int star3Score;
    public List<PieceSpawnData> positions;
}

[Serializable]
public struct PieceSpawnData
{
    public List<Piece> pieces;
    public PieceColour colour;
    public PieceAbility ability;
    public Vector2 position;
}
