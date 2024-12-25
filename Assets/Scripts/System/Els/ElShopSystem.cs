using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ElShopSystem : ElDependency
{
    private ElTimerUISystem _timerUISystem;
    private ElPauseUISystem _pauseUISystem;
    private ElAudioSystem _audioSystem;
    private ElArtefactsUISystem _artefactsUISystem;
    
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
        if (Creator.TryGetDependency(out ElAudioSystem audioSystem))
        {
            _audioSystem = audioSystem;
        }
        if (Creator.TryGetDependency(out ElArtefactsUISystem artefactsSystem))
        {
            _artefactsUISystem = artefactsSystem;
        }
        
        GameObject shop = GameObject.FindWithTag("Shop");

        shop.transform.position += new Vector3(0, 11f * Creator.shopSo.shopRoomNumber, 0);
        
        Transform[] childObjs = shop.GetComponentsInChildren<Transform>();
        foreach (Transform childObj in childObjs)
        {
            if(childObj == shop.transform)
                continue;
            if (childObj.CompareTag("Piece"))
            {
                ShopPiece shopPiece = childObj.GetComponent<ShopPiece>();
                SpriteRenderer pieceRenderer = childObj.GetComponent<SpriteRenderer>();
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
            else if(childObj.CompareTag("Item"))
            {
                GameObject shopItem = Creator.InstantiateGameObjectWithParent(Creator.shopItemPrefab, childObj);
                SpriteRenderer shopItemSprRend = shopItem.GetComponent<SpriteRenderer>();
                TMP_Text costText = shopItem.GetComponentInChildren<TMP_Text>();
                string costTextAsString = "";
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
                                costTextAsString = $"-{Creator.timerSo.artefactsCost[ArtefactTypes.CaptureKingClearRoom]:0.##}s";
                                break;
                            case ArtefactTypes.ConCaptureAttackingEnemy:
                                shopItemSprRend.sprite = Creator.shopSo.destroyChainStayAliveSprite;
                                costTextAsString = $"-{Creator.timerSo.artefactsCost[ArtefactTypes.ConCaptureAttackingEnemy]:0.##}s";
                                break;
                            case ArtefactTypes.EnemyLineOfSight:
                                switch (Creator.shopSo.lineOfSightForArtefact[shopItem.transform.position])
                                {
                                    case Piece.Pawn:
                                        shopItemSprRend.sprite = Creator.shopSo.pawnLosSprite;
                                        break;
                                    case Piece.Knight:
                                        shopItemSprRend.sprite = Creator.shopSo.knightLosSprite;
                                        break;
                                    case Piece.Bishop:
                                        shopItemSprRend.sprite = Creator.shopSo.bishopLosSprite;
                                        break;
                                    case Piece.Rook:
                                        shopItemSprRend.sprite = Creator.shopSo.rookLosSprite;
                                        break;
                                    case Piece.Queen:
                                        shopItemSprRend.sprite = Creator.shopSo.queenLosSprite;
                                        break;
                                    case Piece.King:
                                        shopItemSprRend.sprite = Creator.shopSo.kingLosSprite;
                                        break;
                                }
                                costTextAsString = $"-{Creator.timerSo.artefactsCost[ArtefactTypes.EnemyLineOfSight] * Creator.timerSo.capturePieceTimeAdd[Creator.shopSo.lineOfSightForArtefact[shopItem.transform.position]]:0.##}s";
                                break;
                            case ArtefactTypes.UseCapturedPieceStraightAway:
                                shopItemSprRend.sprite = Creator.shopSo.useCapturedPieceStraightAwaySprite;
                                costTextAsString = $"-{Creator.timerSo.artefactsCost[ArtefactTypes.ConCaptureAttackingEnemy]:0.##}s";
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
                        costTextAsString = $"-{Creator.timerSo.upgradesCost[Creator.shopSo.upgradesPositions[shopItem.transform.position]]:0.##}s";

                        Creator.shopSo.upgradesSprites[shopItem.transform.position] = shopItemSprRend;
                    }
                    else
                    {
                        shopItem.gameObject.SetActive(false);
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
                            ArtefactTypes.ConCaptureAttackingEnemy,
                            ArtefactTypes.UseCapturedPieceStraightAway,
                            ArtefactTypes.CaptureKingClearRoom
                        };
                        int index = Random.Range(0, types.Count);
                        
                        if (types[index] != ArtefactTypes.EnemyLineOfSight && (Creator.shopSo.artefactsPositions.ContainsValue(types[index]) || Creator.playerSystemSo.artefacts.Contains(types[index])))
                        {
                            List<ArtefactTypes> typesRemoved = types;
                            typesRemoved.Remove(types[index]);
                            int removedIndex = Random.Range(0, typesRemoved.Count);
                            
                            while(typesRemoved.Count > 0 && (Creator.shopSo.artefactsPositions.ContainsValue(typesRemoved[removedIndex]) || Creator.playerSystemSo.artefacts.Contains(typesRemoved[removedIndex])))
                            {
                                typesRemoved.Remove(typesRemoved[removedIndex]);
                                removedIndex = Random.Range(0, typesRemoved.Count);
                            }

                            if (typesRemoved.Count == 0)
                            {
                                Debug.LogError("We should not get here!");
                            }
                            else
                            {
                                types = typesRemoved;
                                index = removedIndex;
                            }
                        }
                        
                        Creator.shopSo.artefactsPositions.Add(shopItem.transform.position, types[index]);
                        Creator.shopSo.artefactsSprites.Add(shopItem.transform.position, shopItemSprRend);
                        
                        switch (types[index])
                        {
                            case ArtefactTypes.CaptureKingClearRoom:
                                shopItemSprRend.sprite = Creator.shopSo.captureKingClearRoomSprite;
                                costTextAsString = $"-{Creator.timerSo.artefactsCost[ArtefactTypes.CaptureKingClearRoom]:0.##}s";
                                break;
                            case ArtefactTypes.ConCaptureAttackingEnemy:
                                shopItemSprRend.sprite = Creator.shopSo.destroyChainStayAliveSprite;
                                costTextAsString = $"-{Creator.timerSo.artefactsCost[ArtefactTypes.ConCaptureAttackingEnemy]:0.##}s";
                                break;
                            case ArtefactTypes.EnemyLineOfSight:
                                List<Piece> pieceTypes = new()
                                {
                                    Piece.Pawn,
                                    Piece.Bishop,
                                    Piece.Knight,
                                    Piece.Rook,
                                    Piece.Queen,
                                    Piece.King
                                };
                                foreach (Piece losOwned in Creator.playerSystemSo.lineOfSightsChosen)
                                {
                                    pieceTypes.Remove(losOwned);
                                }
                                
                                int pieceTypeIndex = Random.Range(0, pieceTypes.Count);
                                Creator.shopSo.lineOfSightForArtefact.Add(shopItem.transform.position, pieceTypes[pieceTypeIndex]);
                                switch (Creator.shopSo.lineOfSightForArtefact[shopItem.transform.position])
                                {
                                    case Piece.Pawn:
                                        shopItemSprRend.sprite = Creator.shopSo.pawnLosSprite;
                                        break;
                                    case Piece.Knight:
                                        shopItemSprRend.sprite = Creator.shopSo.knightLosSprite;
                                        break;
                                    case Piece.Bishop:
                                        shopItemSprRend.sprite = Creator.shopSo.bishopLosSprite;
                                        break;
                                    case Piece.Rook:
                                        shopItemSprRend.sprite = Creator.shopSo.rookLosSprite;
                                        break;
                                    case Piece.Queen:
                                        shopItemSprRend.sprite = Creator.shopSo.queenLosSprite;
                                        break;
                                    case Piece.King:
                                        shopItemSprRend.sprite = Creator.shopSo.kingLosSprite;
                                        break;
                                }
                                costTextAsString = $"-{Creator.timerSo.artefactsCost[ArtefactTypes.EnemyLineOfSight] * Creator.timerSo.capturePieceTimeAdd[Creator.shopSo.lineOfSightForArtefact[shopItem.transform.position]]:0.##}s";
                                break;
                            case ArtefactTypes.UseCapturedPieceStraightAway:
                                shopItemSprRend.sprite = Creator.shopSo.useCapturedPieceStraightAwaySprite;
                                costTextAsString = $"-{Creator.timerSo.artefactsCost[ArtefactTypes.UseCapturedPieceStraightAway]:0.##}s";
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
                        costTextAsString = $"-{Creator.timerSo.upgradesCost[types[index]]:0.##}s";
                    }
                }

                costText.text = costTextAsString;
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
                Piece piece = Piece.NotChosen;
                if (Creator.shopSo.lineOfSightForArtefact.TryGetValue(pos, out Piece value))
                {
                    piece = value;
                }
                if(!_artefactsUISystem.TryAddArtefact(Creator.shopSo.artefactsPositions[pos], piece))
                {
                    //TODO: ADD 'error' sfx
                    return;
                }
                _audioSystem.PlayerLevelUpSfx();
                float cost = Creator.timerSo.artefactsCost[Creator.shopSo.artefactsPositions[pos]];
                switch (Creator.shopSo.artefactsPositions[pos])
                {
                    case ArtefactTypes.CaptureKingClearRoom:
                        break;
                    case ArtefactTypes.ConCaptureAttackingEnemy:
                        break;
                    case ArtefactTypes.EnemyLineOfSight:
                        cost *= Creator.timerSo.capturePieceTimeAdd[piece];
                        Creator.playerSystemSo.lineOfSightsChosen.Add(piece);
                        break;
                    case ArtefactTypes.UseCapturedPieceStraightAway:
                        break;
                }
                _timerUISystem.RemoveTime(cost, false);
                
                Creator.shopSo.artefactsSprites[pos].gameObject.SetActive(false);
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
                _audioSystem.PlayerLevelUpSfx();
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
                        List<Piece> promoPieces = Creator.timerSo.timeCost.Keys.ToList();
                        foreach (Piece piece in promoPieces)
                        {
                            Creator.timerSo.timeCost[piece] -= 1;
                            if (Creator.timerSo.timeCost[piece] <= 0)
                            {
                                Creator.timerSo.timeCost[piece] = 0;
                            }
                        }
                        break;
                    case UpgradeTypes.ReduceRespawnCost:
                        Creator.timerSo.playerRespawnDivideCost += 1;
                        break;
                }
                
                int cost = Creator.timerSo.upgradesCost[Creator.shopSo.upgradesPositions[pos]];
                _timerUISystem.RemoveTime(cost, false);
                
                Creator.shopSo.upgradesSprites[pos].gameObject.SetActive(false);
                Creator.shopSo.upgradesPositions.Remove(pos);
                Creator.shopSo.itemsTakenInLevelCount++;
                _pauseUISystem.UpdateTextInfo();
                _pauseUISystem.ShowUpgradeNotificationImage();
                return;
            }
        }
    }
}
