using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class XPBar_SO : ScriptableObject
{
    [FormerlySerializedAs("reducePromotionCostSprite")] [Header("Upgrades")]
    public Sprite increasePromoXpGainSprite;
    public Sprite increaseMultiplierAmountSprite;
    public Sprite increaseBaseAmountGainedSprite;
    public Sprite reduceRespawnCostSprite;

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

    public Sprite xpLevelUpgradeSprite;
    
    public Dictionary<Piece, float> capturePieceXPGain = new()
    {
        {Piece.Pawn, 1},
        {Piece.Bishop, 3},
        {Piece.Knight, 3},
        {Piece.Rook, 5},
        {Piece.Queen, 9},
        {Piece.King, 9}
    };
    
    public Dictionary<Piece, float> promotionXPGain = new()
    {
        {Piece.Bishop, 5},
        {Piece.Knight, 5},
        {Piece.Rook, 3},
        {Piece.Queen, 1},
    };

    public int levelNumberOnRoomEnter = 1;
    public int levelNumber = 1;

    public float baseMultiplier = 1;

    public List<ArtefactTypes> artefactsChosen = new();
    
    public List<Piece> lineOfSightsChosen = new();

    public bool guaranteeArtefactInUpgrade;
    
    public void ResetData()
    {
        capturePieceXPGain = new()
        {
            { Piece.Pawn, 1 },
            { Piece.Bishop, 3 },
            { Piece.Knight, 3 },
            { Piece.Rook, 5 },
            { Piece.Queen, 9 },
            { Piece.King, 9 }
        };

        promotionXPGain = new()
        {
            { Piece.Bishop, 5 },
            { Piece.Knight, 5 },
            { Piece.Rook, 3 },
            { Piece.Queen, 1 },
        };

        levelNumberOnRoomEnter = 1;
        levelNumber = 1;
        baseMultiplier = 1;
        
        artefactsChosen.Clear();
        
        lineOfSightsChosen.Clear();
    }
}
