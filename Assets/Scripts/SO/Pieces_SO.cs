using UnityEngine;

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
    
    [Header("AI-Specific")]
    public Material noneMat;
    public Material mustMoveMat;
    public Material glitchedMat;
    public Material captureLoverMat;

    public float aiThinkingTime;
    [Range(1,100)] public int capturePlayerOdds;
}
