using System;
using System.Collections.Generic;
using UnityEngine;

public enum PieceColour
{
    Black,
    White
}

[Serializable]
public struct Level
{
    public int number;
    public int turns;
    public List<PieceSpawnData> positions;
}

[Serializable]
public struct PieceSpawnData
{
    public Piece piece;
    public PieceColour colour;
    public Vector2 position;
}
