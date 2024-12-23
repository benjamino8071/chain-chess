using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu]
public class Shop_SO : ScriptableObject
{
    public int shopRoomNumber;
    
    //Upgrade sprites
    [Header("Upgrades")]
    public Sprite reducePromotionCostSprite;
    public Sprite increaseMultiplierAmountSprite;
    public Sprite increaseBaseAmountGainedSprite;
    public Sprite reduceRespawnCostSprite;

    //Artefact sprites
    [Header("Artefacts")]
    public Sprite pawnLosSprite;
    public Sprite knightLosSprite;
    public Sprite bishopLosSprite;
    public Sprite rookLosSprite;
    public Sprite queenLosSprite;
    public Sprite kingLosSprite;
    
    public Sprite destroyChainStayAliveSprite;
    public Sprite useCapturedPieceStraightAwaySprite;
    public Sprite captureKingClearRoomSprite;

    [NonSerialized] public Dictionary<Vector3, UpgradeTypes> upgradesPositions = new();
    [NonSerialized] public Dictionary<Vector3, SpriteRenderer> upgradesSprites = new();
    [NonSerialized] public Dictionary<Vector3, ArtefactTypes> artefactsPositions = new();
    [NonSerialized] public Dictionary<Vector3, SpriteRenderer> artefactsSprites = new();
    [NonSerialized] public List<ArtefactTypes> artefactsChosen = new(3);
    [NonSerialized] public int itemsTakenInLevelCount;
    
    //Data for artefact types
    [NonSerialized] public Dictionary<Vector3, Piece> lineOfSightForArtefact = new();

    public void ResetData()
    {
        upgradesPositions.Clear();
        upgradesSprites.Clear();
        artefactsPositions.Clear();
        artefactsSprites.Clear();
        artefactsChosen.Clear();
        lineOfSightForArtefact.Clear();
        itemsTakenInLevelCount = 0;
        Debug.Log("Shop_SO caches cleared");
    }
}
