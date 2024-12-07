using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ElChainUISystem : ElDependency
{
    private Transform _chainParent;
    
    private Vector3 _chainParentInitialPos;
    
    private LinkedList<RectTransform> _chainPiecesImages = new ();
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _chainParent = GameObject.FindWithTag("ChainParent").transform;

        _chainParentInitialPos = _chainParent.position;
        
        ResetPosition();
        ShowNewPiece(Creator.startingPiece, true);
    }

    public void ShowNewPiece(Piece piece, bool isFirstPiece = false)
    {
        GameObject newPieceImage;
        if (_chainPiecesImages.Count != 0)
        {
            RectTransform lastPieceTransform = _chainPiecesImages.Last.Value;

            newPieceImage = Creator.InstantiateGameObject(Creator.capturedPieceImagePrefab,
                lastPieceTransform.position + new Vector3(150, 0, 0), Quaternion.identity);
        }
        else
        {
            newPieceImage = Creator.InstantiateGameObject(Creator.capturedPieceImagePrefab,
                _chainParent.position, Quaternion.identity);
        }

        Image visual = newPieceImage.GetComponentInChildren<Image>();
        
        //Set the sprite based on the piece
        switch (piece)
        {
            case Piece.Pawn:
                visual.sprite = Creator.playerSystemSo.pawn;
                break;
            case Piece.Rook:
                visual.sprite = Creator.playerSystemSo.rook;
                break;
            case Piece.Knight:
                visual.sprite = Creator.playerSystemSo.knight;
                break;
            case Piece.Bishop:
                visual.sprite = Creator.playerSystemSo.bishop;
                break;
            case Piece.Queen:
                visual.sprite = Creator.playerSystemSo.queen;
                break;
            case Piece.King:
                visual.sprite = Creator.playerSystemSo.king;
                break;
        }
        
        newPieceImage.transform.SetParent(_chainParent, true);

        if (!isFirstPiece)
        {
            Vector3 posBehindNewPieceImage = newPieceImage.transform.position - new Vector3(75, 0, 0);
            GameObject arrowPointingToNextPiece = Creator.InstantiateGameObject(Creator.arrowPointingToNextPiecePrefab,
                posBehindNewPieceImage, Quaternion.identity);
            
            arrowPointingToNextPiece.transform.SetParent(_chainParent, true);

            _chainPiecesImages.AddLast(arrowPointingToNextPiece.GetComponent<RectTransform>());
        }
        
        _chainPiecesImages.AddLast(newPieceImage.GetComponent<RectTransform>());
    }

    public void ResetPosition()
    {
        _chainParent.position = _chainParentInitialPos;
    }

    public void NewRoomClearChain()
    {
        foreach (RectTransform capturedPiecesImage in _chainPiecesImages)
        {
            capturedPiecesImage.gameObject.SetActive(false);
        }
        _chainPiecesImages.Clear();
    }

    public void HighlightNextPiece()
    {
        //Move _capturedPiecesParent along
        _chainParent.position += new Vector3(-150, 0, 0);
    }
}
