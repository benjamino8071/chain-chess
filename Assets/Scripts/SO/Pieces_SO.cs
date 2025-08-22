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
    [Range(1,100)] public int capturePlayerOdds;
    
    private SystemRandom _systemRand = new(42);

    public void ResetSystemRandom()
    {
        _systemRand = new(42);
    }
    
    public Piece GetRandomPiece()
    {
        return (Piece)_systemRand.Next(1, 6);
    }
}
