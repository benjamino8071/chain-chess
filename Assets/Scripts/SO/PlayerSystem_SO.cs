using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class PlayerSystem_SO : ScriptableObject
{
    public float moveSpeed;
    public float walkThroughDoorSpeed;

    public Sprite pawn;
    public Sprite rook;
    public Sprite knight;
    public Sprite bishop;
    public Sprite queen;
    public Sprite king;

    //The room number is saved after a player enters a room and can move around
    public int levelNumberSaved;
    public int roomNumberSaved;
    public Piece startingPiece = Piece.NotChosen;
}
