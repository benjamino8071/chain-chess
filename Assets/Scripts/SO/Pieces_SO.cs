using UnityEngine;

[CreateAssetMenu]
public class Pieces_SO : ScriptableObject
{
    public Sprite pawn;
    public Sprite rook;
    public Sprite knight;
    public Sprite bishop;
    public Sprite queen;
    public Sprite king;

    public Color whiteColor;
    public Color blackColor;

    public float aiThinkingTime;
}
