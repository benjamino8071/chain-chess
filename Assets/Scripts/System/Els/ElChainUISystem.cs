using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElChainUISystem : ElDependency
{
    private Transform _chainParent;

    private TextMeshProUGUI _movesRemainingText;
    
    private Vector3 _chainParentInitialPos;
    
    private LinkedList<(Piece, RectTransform, Image)> _chainPiecesImages = new ();
    private LinkedListNode<(Piece, RectTransform, Image)> _currentPiece;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        _chainParent = GameObject.FindWithTag("ChainParent").transform;

        GameObject movesRemainingText = GameObject.FindWithTag("MovesRemaining");
        _movesRemainingText = movesRemainingText.GetComponentInChildren<TextMeshProUGUI>();
        
        _chainParentInitialPos = _chainParent.localPosition;
        
        ResetPosition();
        
        ShowNewPiece(Creator.playerSystemSo.startingPiece, true);
        
        UpdateMovesRemainingText(1);
    }
    
    public void ShowNewPiece(Piece piece, bool isFirstPiece = false)
    {
        GameObject newPieceImage;
        if (_chainPiecesImages.Count != 0)
        {
            RectTransform lastPieceTransform = _chainPiecesImages.Last.Value.Item2;

            newPieceImage = Creator.InstantiateGameObjectWithParent(Creator.capturedPieceImagePrefab, _chainParent);

            newPieceImage.transform.localPosition += lastPieceTransform.localPosition + new Vector3(250, 0, 0);
        }
        else
        {
            newPieceImage = Creator.InstantiateGameObjectWithParent(Creator.capturedPieceImagePrefab, _chainParent);
        }

        Image visual = newPieceImage.GetComponent<Image>();

        visual.sprite = GetSprite(piece);
        
        //For every other piece we first want to add an arrow indicating the order for the chain
        if (!isFirstPiece)
        {
            Vector3 posBehindNewPieceImage = newPieceImage.transform.localPosition - new Vector3(125, 0, 0);

            GameObject arrowPointingToNextPiece = Creator.InstantiateGameObjectWithParent(Creator.arrowPointingToNextPiecePrefab, _chainParent);

            arrowPointingToNextPiece.transform.localPosition = posBehindNewPieceImage;

            _chainPiecesImages.AddLast((Piece.NotChosen, arrowPointingToNextPiece.GetComponent<RectTransform>(), visual));
        }
        
        _chainPiecesImages.AddLast((piece, newPieceImage.GetComponent<RectTransform>(), visual));
        if (isFirstPiece)
        {
            _currentPiece = _chainPiecesImages.First;
        }
    }

    public void ResetPosition()
    {
        _chainParent.localPosition = _chainParentInitialPos;
        _currentPiece = _chainPiecesImages.First;
        foreach ((Piece, RectTransform, Image) chainPiecesImage in _chainPiecesImages)
        {
            chainPiecesImage.Item3.color = new Color(1,1,1, 1);
        }
    }

    public void NewRoomClearChain()
    {
        foreach ((Piece, RectTransform, Image) capturedPiecesImage in _chainPiecesImages)
        {
            capturedPiecesImage.Item2.gameObject.SetActive(false);
        }
        _chainPiecesImages.Clear();
        _currentPiece = null;
    }

    public void HighlightNextPiece()
    {
        //Move _capturedPiecesParent along
        _currentPiece.Value.Item3.color = new Color(0.75f,0.75f,0.75f,1);
        _currentPiece = _currentPiece.Next.Next;
        _chainParent.localPosition += new Vector3(-250, 0, 0);
    }

    public void PawnPromoted(int index, Piece promotedPiece)
    {
        LinkedListNode<(Piece, RectTransform, Image)> temp = _chainPiecesImages.First;
        int tempIndex = 0;
        while (temp != null)
        {
            if (index == tempIndex)
            {
                (Piece, RectTransform, Image) value = temp.Value;
                value.Item1 = promotedPiece;
                value.Item3.sprite = GetSprite(promotedPiece);
                temp.Value = value;
                break;
            }

            tempIndex++;
            temp = temp.Next;
        }
    }

    private Sprite GetSprite(Piece piece)
    {
        switch (piece)
        {
            case Piece.Pawn:
                return Creator.playerSystemSo.pawn;
            case Piece.Rook:
                return Creator.playerSystemSo.rook;
            case Piece.Knight:
                return Creator.playerSystemSo.knight;
            case Piece.Bishop:
                return Creator.playerSystemSo.bishop;
            case Piece.Queen:
                return Creator.playerSystemSo.queen;
            case Piece.King:
                return Creator.playerSystemSo.king;
        }
        
        //Given the logic of the code we should never get here but have to add something
        return default;
    }

    public void UpdateMovesRemainingText(int movesRemaining)
    {
        _movesRemainingText.text = movesRemaining.ToString();
    }
}
