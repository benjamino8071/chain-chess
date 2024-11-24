using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CapturedPiecesUISystem : Dependency
{
    private Transform _capturedPiecesParent;
    
    private Vector3 _capturedPiecesParentInitialPos;
    
    private LinkedList<RectTransform> _capturedPiecesImages = new ();

    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

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

            newPieceImage = _creator.InstantiateGameObject(_creator.capturedPieceImagePrefab,
                lastPieceTransform.position + new Vector3(150, 0, 0), Quaternion.identity);
        }
        else
        {
            newPieceImage = _creator.InstantiateGameObject(_creator.capturedPieceImagePrefab,
                _capturedPiecesParent.position, Quaternion.identity);
        }

        Image visual = newPieceImage.GetComponentInChildren<Image>();
        
        //Set the sprite based on the piece
        switch (piece)
        {
            case Piece.Pawn:
                visual.sprite = _creator.playerSystemSo.pawn;
                break;
            case Piece.Rook:
                visual.sprite = _creator.playerSystemSo.rook;
                break;
            case Piece.Knight:
                visual.sprite = _creator.playerSystemSo.knight;
                break;
            case Piece.Bishop:
                visual.sprite = _creator.playerSystemSo.bishop;
                break;
            case Piece.Queen:
                visual.sprite = _creator.playerSystemSo.queen;
                break;
            case Piece.King:
                visual.sprite = _creator.playerSystemSo.king;
                break;
        }
        
        newPieceImage.transform.SetParent(_capturedPiecesParent, true);

        if (!isFirstPiece)
        {
            Vector3 posBehindNewPieceImage = newPieceImage.transform.position - new Vector3(75, 0, 0);
            GameObject arrowPointingToNextPiece = _creator.InstantiateGameObject(_creator.arrowPointingToNextPiecePrefab,
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
