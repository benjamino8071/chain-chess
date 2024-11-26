using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CapturedPiecesUISystem : ElDependency
{
    private Transform _capturedPiecesParent;
    
    private Vector3 _capturedPiecesParentInitialPos;
    
    private LinkedList<RectTransform> _capturedPiecesImages = new ();

    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        _capturedPiecesParent = GameObject.FindWithTag("CapturedPiecesParent").transform;

        _capturedPiecesParentInitialPos = _capturedPiecesParent.position;
        
        Reset();
    }

    public void ShowNewPiece(Piece piece, bool isFirstPiece = false)
    {
        GameObject newPieceImage;
        if (_capturedPiecesImages.Count != 0)
        {
            RectTransform lastPieceTransform = _capturedPiecesImages.Last.Value;

            newPieceImage = Creator.InstantiateGameObject(Creator.capturedPieceImagePrefab,
                lastPieceTransform.position + new Vector3(150, 0, 0), Quaternion.identity);
        }
        else
        {
            newPieceImage = Creator.InstantiateGameObject(Creator.capturedPieceImagePrefab,
                _capturedPiecesParent.position, Quaternion.identity);
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
        
        newPieceImage.transform.SetParent(_capturedPiecesParent, true);

        if (!isFirstPiece)
        {
            Vector3 posBehindNewPieceImage = newPieceImage.transform.position - new Vector3(75, 0, 0);
            GameObject arrowPointingToNextPiece = Creator.InstantiateGameObject(Creator.arrowPointingToNextPiecePrefab,
                posBehindNewPieceImage, Quaternion.identity);
            
            arrowPointingToNextPiece.transform.SetParent(_capturedPiecesParent, true);

            _capturedPiecesImages.AddLast(arrowPointingToNextPiece.GetComponent<RectTransform>());
        }
        
        _capturedPiecesImages.AddLast(newPieceImage.GetComponent<RectTransform>());
    }

    public void Reset()
    {
        _capturedPiecesParent.position = _capturedPiecesParentInitialPos;
    }

    public void InNewRoomReset()
    {
        foreach (RectTransform capturedPiecesImage in _capturedPiecesImages)
        {
            capturedPiecesImage.gameObject.SetActive(false);
        }
        _capturedPiecesImages.Clear();
    }

    public void HighlightNextPiece()
    {
        //Move _capturedPiecesParent along
        _capturedPiecesParent.position += new Vector3(-150, 0, 0);
    }
}
