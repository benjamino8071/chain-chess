using System;
using System.Collections.Generic;
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
    CaptureLover
}

[Serializable]
public struct Level
{
    public int level;
    public int turns;
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
