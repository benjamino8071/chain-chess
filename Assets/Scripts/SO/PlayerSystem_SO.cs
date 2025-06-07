using System.Collections.Generic;
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

    public Material noEffectMaterial;
    public Material invincibleEffectMaterial;
    public Material superInvincibleEffectMaterial;
    
    //The room number is saved after a player enters a room and can move around
    public int levelNumberSaved;
    public float xValueToStartOn;
    public int roomNumberSaved;
    public bool hideMainMenuTrigger;
    public bool moveMadeInNewRoom;
    public Piece startingPiece = Piece.NotChosen;
    public List<ArtefactTypes> artefacts = new();
    
    //Data for the artefact types
    public List<Piece> lineOfSightsChosen = new();

    public bool allowPlayerToChoosePromotion;
    public bool keepChainWhenRoomCleared;
    
    public void ResetData()
    {
        levelNumberSaved = 0;
        roomNumberSaved = 0;
        xValueToStartOn = 5.5f;
        moveMadeInNewRoom = false;
        artefacts.Clear();
        lineOfSightsChosen.Clear();
        Debug.Log("PlayerSystem_SO cache reset");
    }
}
