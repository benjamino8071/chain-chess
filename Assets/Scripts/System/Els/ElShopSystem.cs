using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ElShopSystem : ElDependency
{
    private ElTimerUISystem _timerUISystem;
    private ElPauseUISystem _pauseUISystem;
    
    public enum UpgradeTypes
    {
        ReducePromotionCost,
        IncreaseMultiplierAmount,
        IncreaseBaseAmountGained,
        ReduceRespawnCost
    }
    
    public enum ArtefactTypes
    {
        EnemyLineOfSight,
        DestroyChainStayAlive,
        UseCapturedPieceStraightAway,
        CaptureKingClearRoom
    }

    private readonly Dictionary<UpgradeTypes, int> _upgradesCost = new()
    {
        { UpgradeTypes.ReducePromotionCost, 0 },
        { UpgradeTypes.IncreaseMultiplierAmount, 0},
        { UpgradeTypes.IncreaseBaseAmountGained , 0},
        { UpgradeTypes.ReduceRespawnCost, 0}
    };

    private readonly Dictionary<ArtefactTypes, int> _artefactsCost = new()
    {
        { ArtefactTypes.EnemyLineOfSight, 0},
        { ArtefactTypes.DestroyChainStayAlive, 0},
        { ArtefactTypes.UseCapturedPieceStraightAway, 0},
        { ArtefactTypes.CaptureKingClearRoom, 0}
    };
    
    private Dictionary<Vector3, ShopPiece> _shopPiecesPositions = new();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (Creator.TryGetDependency(out ElTimerUISystem timerUISystem))
        {
            _timerUISystem = timerUISystem;
        }
        if (Creator.TryGetDependency(out ElPauseUISystem pauseUISystem))
        {
            _pauseUISystem = pauseUISystem;
        }
        
        GameObject shop = GameObject.FindWithTag("Shop");

        shop.transform.position += new Vector3(0, 11f * Creator.shopSo.shopRoomNumber, 0);
        
        Transform[] childObjs = shop.GetComponentsInChildren<Transform>();
        foreach (Transform childObj in childObjs)
        {
            if(childObj == shop.transform)
                continue;
            if (childObj.TryGetComponent(out ShopPiece shopPiece))
            {
                SpriteRenderer pieceRenderer = shopPiece.GetComponent<SpriteRenderer>();
                switch (shopPiece.piece)
                {
                    case Piece.Rook:
                        pieceRenderer.sprite = Creator.playerSystemSo.rook;
                        break;
                    case Piece.Bishop:
                        pieceRenderer.sprite = Creator.playerSystemSo.bishop;
                        break;
                    case Piece.Knight:
                        pieceRenderer.sprite = Creator.playerSystemSo.knight;
                        break;
                    case Piece.Queen:
                        pieceRenderer.sprite = Creator.playerSystemSo.queen;
                        break;
                }
                _shopPiecesPositions.Add(childObj.position, shopPiece);
            }
            else
            {
                GameObject shopItem = Creator.InstantiateGameObjectWithParent(Creator.shopItemPrefab, childObj);
                SpriteRenderer shopItemSprRend = shopItem.GetComponent<SpriteRenderer>();
                
                //So if the player has respawned, then we do NOT want to change the cache
                if (Creator.shopSo.artefactsPositions.Count + Creator.shopSo.upgradesPositions.Count + Creator.shopSo.itemsTakenInLevelCount >= 4)
                {
                    //Just set the right sprite to shopItem based on whether it is in artefacts or upgrades
                    if (Creator.shopSo.artefactsPositions.ContainsKey(shopItem.transform.position))
                    {
                        switch (Creator.shopSo.artefactsPositions[shopItem.transform.position])
                        {
                            case ArtefactTypes.CaptureKingClearRoom:
                                shopItemSprRend.sprite = Creator.shopSo.captureKingClearRoomSprite;
                                break;
                            case ArtefactTypes.DestroyChainStayAlive:
                                shopItemSprRend.sprite = Creator.shopSo.destroyChainStayAliveSprite;
                                break;
                            case ArtefactTypes.EnemyLineOfSight:
                                shopItemSprRend.sprite = Creator.shopSo.enemyLineOfSightSprite;
                                break;
                            case ArtefactTypes.UseCapturedPieceStraightAway:
                                shopItemSprRend.sprite = Creator.shopSo.useCapturedPieceStraightAwaySprite;
                                break;
                        }
                        Creator.shopSo.artefactsSprites[shopItem.transform.position] = shopItemSprRend;
                    }
                    else if (Creator.shopSo.upgradesPositions.ContainsKey(shopItem.transform.position))
                    {
                        switch (Creator.shopSo.upgradesPositions[shopItem.transform.position])
                        {
                            case UpgradeTypes.IncreaseMultiplierAmount:
                                shopItemSprRend.sprite = Creator.shopSo.increaseMultiplierAmountSprite;
                                break;
                            case UpgradeTypes.IncreaseBaseAmountGained:
                                shopItemSprRend.sprite = Creator.shopSo.increaseBaseAmountGainedSprite;
                                break;
                            case UpgradeTypes.ReducePromotionCost:
                                shopItemSprRend.sprite = Creator.shopSo.reducePromotionCostSprite;
                                break;
                            case UpgradeTypes.ReduceRespawnCost:
                                shopItemSprRend.sprite = Creator.shopSo.reduceRespawnCostSprite;
                                break;
                        }
                        Creator.shopSo.upgradesSprites[shopItem.transform.position] = shopItemSprRend;
                    }
                }
                else
                {
                    int isArtefact = Random.Range(0, 2);
                    
                    if (isArtefact == 1)
                    {
                        List<ArtefactTypes> types = new()
                        {
                            ArtefactTypes.EnemyLineOfSight,
                            ArtefactTypes.DestroyChainStayAlive,
                            ArtefactTypes.UseCapturedPieceStraightAway,
                            ArtefactTypes.CaptureKingClearRoom
                        };
                        int index = Random.Range(0, types.Count);
                        
                        Creator.shopSo.artefactsPositions.Add(shopItem.transform.position, types[index]);
                        Creator.shopSo.artefactsSprites.Add(shopItem.transform.position, shopItemSprRend);
                        
                        switch (types[index])
                        {
                            case ArtefactTypes.CaptureKingClearRoom:
                                shopItemSprRend.sprite = Creator.shopSo.captureKingClearRoomSprite;
                                break;
                            case ArtefactTypes.DestroyChainStayAlive:
                                shopItemSprRend.sprite = Creator.shopSo.destroyChainStayAliveSprite;
                                break;
                            case ArtefactTypes.EnemyLineOfSight:
                                shopItemSprRend.sprite = Creator.shopSo.enemyLineOfSightSprite;
                                break;
                            case ArtefactTypes.UseCapturedPieceStraightAway:
                                shopItemSprRend.sprite = Creator.shopSo.useCapturedPieceStraightAwaySprite;
                                break;
                        }
                    }
                    else
                    {
                        List<UpgradeTypes> types = new()
                        {
                            UpgradeTypes.ReducePromotionCost,
                            UpgradeTypes.IncreaseMultiplierAmount,
                            UpgradeTypes.IncreaseBaseAmountGained,
                            UpgradeTypes.ReduceRespawnCost
                        };
                        int index = Random.Range(0, types.Count);
                        
                        Creator.shopSo.upgradesPositions.Add(shopItem.transform.position, types[index]);
                        Creator.shopSo.upgradesSprites.Add(shopItem.transform.position, shopItemSprRend);
                        
                        switch (types[index])
                        {
                            case UpgradeTypes.IncreaseMultiplierAmount:
                                shopItemSprRend.sprite = Creator.shopSo.increaseMultiplierAmountSprite;
                                break;
                            case UpgradeTypes.IncreaseBaseAmountGained:
                                shopItemSprRend.sprite = Creator.shopSo.increaseBaseAmountGainedSprite;
                                break;
                            case UpgradeTypes.ReducePromotionCost:
                                shopItemSprRend.sprite = Creator.shopSo.reducePromotionCostSprite;
                                break;
                            case UpgradeTypes.ReduceRespawnCost:
                                shopItemSprRend.sprite = Creator.shopSo.reduceRespawnCostSprite;
                                break;
                        }
                    }
                }
            }
        }
    }

    public bool TryGetShopPieceAtPosition(Vector3 playerPosition, out Piece piece)
    {
        foreach (Vector3 pos in _shopPiecesPositions.Keys)
        {
            if ((int)pos.x == (int)playerPosition.x && (int)pos.y == (int)playerPosition.y)
            {
                piece = _shopPiecesPositions[pos].piece;
                return true;
            }
        }
        
        piece = Creator.playerSystemSo.startingPiece;
        return false;
    }

    public void TryGetArtefactAtPosition(Vector3 playerPosition)
    {
        foreach (Vector3 pos in Creator.shopSo.artefactsPositions.Keys)
        {
            if ((int)pos.x == (int)playerPosition.x && (int)pos.y == (int)playerPosition.y)
            {
                switch (Creator.shopSo.artefactsPositions[pos])
                {
                    case ArtefactTypes.CaptureKingClearRoom:
                        break;
                    case ArtefactTypes.DestroyChainStayAlive:
                        break;
                    case ArtefactTypes.EnemyLineOfSight:
                        break;
                    case ArtefactTypes.UseCapturedPieceStraightAway:
                        break;
                }
                
                int cost = _artefactsCost[Creator.shopSo.artefactsPositions[pos]];
                _timerUISystem.RemoveTime(cost);
                
                Creator.shopSo.artefactsSprites[pos].sprite = default;
                Creator.shopSo.artefactsPositions.Remove(pos);
                Creator.shopSo.itemsTakenInLevelCount++;
                _pauseUISystem.UpdateTextInfo();
                return;
            }
        }
    }

    public void TryGetUpgradeAtPosition(Vector3 playerPosition)
    {
        foreach (Vector3 pos in Creator.shopSo.upgradesPositions.Keys)
        {
            if ((int)pos.x == (int)playerPosition.x && (int)pos.y == (int)playerPosition.y)
            {
                switch (Creator.shopSo.upgradesPositions[pos])
                {
                    case UpgradeTypes.IncreaseMultiplierAmount:
                        Creator.timerSo.timerMultiplierMultiplier += 0.11f;
                        break;
                    case UpgradeTypes.IncreaseBaseAmountGained:
                        List<Piece> enemyPieces = Creator.timerSo.capturePieceTimeAdd.Keys.ToList();
                        foreach (Piece piece in enemyPieces)
                        {
                            Creator.timerSo.capturePieceTimeAdd[piece] += 1;
                        }
                        break;
                    case UpgradeTypes.ReducePromotionCost:
                        List<Piece> promoPieces = Creator.timerSo.capturePieceTimeAdd.Keys.ToList();
                        foreach (Piece piece in promoPieces)
                        {
                            Creator.timerSo.capturePieceTimeAdd[piece] -= 1;
                        }
                        break;
                    case UpgradeTypes.ReduceRespawnCost:
                        Creator.timerSo.playerRespawnDivideCost += 1;
                        break;
                }
                
                int cost = _upgradesCost[Creator.shopSo.upgradesPositions[pos]];
                _timerUISystem.RemoveTime(cost);
                
                Creator.shopSo.upgradesSprites[pos].sprite = default;
                Creator.shopSo.upgradesPositions.Remove(pos);
                Creator.shopSo.itemsTakenInLevelCount++;
                _pauseUISystem.UpdateTextInfo();
                return;
            }
        }
    }
}
