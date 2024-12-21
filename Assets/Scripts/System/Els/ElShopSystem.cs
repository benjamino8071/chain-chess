using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElShopSystem : ElDependency
{
    private Dictionary<Vector3, ShopPiece> _shopPiecesPositions = new();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
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
                Creator.InstantiateGameObjectWithParent(Creator.shopItemPrefab, childObj);
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
}
