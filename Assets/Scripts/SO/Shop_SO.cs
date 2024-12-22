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
    public Sprite enemyLineOfSightSprite;
    public Sprite destroyChainStayAliveSprite;
    public Sprite useCapturedPieceStraightAwaySprite;
    public Sprite captureKingClearRoomSprite;

    [NonSerialized] public Dictionary<Vector3, ElShopSystem.UpgradeTypes> upgradesPositions = new();
    [NonSerialized] public Dictionary<Vector3, SpriteRenderer> upgradesSprites = new();
    [NonSerialized] public Dictionary<Vector3, ElShopSystem.ArtefactTypes> artefactsPositions = new();
    [NonSerialized] public Dictionary<Vector3, SpriteRenderer> artefactsSprites = new();
    [NonSerialized] public List<ElShopSystem.ArtefactTypes> artefactsChosen = new(3);
    [NonSerialized] public int itemsTakenInLevelCount;

    public void ResetData()
    {
        upgradesPositions.Clear();
        upgradesSprites.Clear();
        artefactsPositions.Clear();
        artefactsSprites.Clear();
        artefactsChosen.Clear();
        itemsTakenInLevelCount = 0;
        Debug.Log("Shop_SO caches cleared");
    }
}
