using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevChainUISystem : LevDependency
{
    private LevTurnSystem _turnSystem;
    
    private Transform _guiTopChain;
    
    private Transform _chainParent;

    private TextMeshProUGUI _movesRemainingText;
    
    private Vector3 _chainParentInitialPos;
    private Vector3 _chainParentNewPos;
    
    private LinkedList<(Piece, Image)> _chainPiecesImages = new ();
    private LinkedListNode<(Piece, Image)> _nextFreeImage;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        
        _guiTopChain = levCreator.GetFirstObjectWithName(AllTagNames.GUITopChain);
        
        _chainParent = levCreator.GetChildObjectByName(_guiTopChain.gameObject, AllTagNames.ChainParent);
        
        Image[] chainPieceImages = _chainParent.GetComponentsInChildren<Image>();
        foreach (Image chainPieceImage in chainPieceImages)
        {
            _chainPiecesImages.AddLast((Piece.NotChosen, chainPieceImage));
            chainPieceImage.gameObject.SetActive(false);
        }
        _nextFreeImage = _chainPiecesImages.First;

        Transform movesRemainingText = levCreator.GetFirstObjectWithName(AllTagNames.MovesRemaining);
        _movesRemainingText = movesRemainingText.GetComponentInChildren<TextMeshProUGUI>();
        
        _chainParentInitialPos = _chainParent.localPosition;
        _chainParentNewPos = _chainParent.localPosition;

        if (SceneManager.sceneCount == 1)
        {
            Show();
        }
        else
        {
            Hide();
        }
        
        SceneManager.sceneUnloaded += SceneManager_SceneUnloaded;
    }

    public override void GameUpdate(float dt)
    {
        if (_chainParentNewPos == _chainParent.localPosition)
        {
            return;
        }
        
        Vector3 lerpPos = math.lerp(_chainParent.localPosition, _chainParentNewPos, dt * Creator.chainSo.addPieceLerpSpeed);
        _chainParent.localPosition = lerpPos;
    }

    public void SetChain(List<Piece> pieces)
    {
        UnsetChain();
        
        for (int i = 0; i < pieces.Count; i++)
        {
            ShowNewPiece(pieces[i], i == 0);
        }

        UpdateMovesRemainingText(pieces.Count);
    }

    public void UnsetChain()
    {
        ResetPosition();
        ClearChain();
        UpdateMovesRemainingText(0);
    }
    
    public void ShowNewPiece(Piece piece, bool isFirstPiece = false)
    {
        //For every other piece we first want to add an arrow indicating the order for the chain
        if (!isFirstPiece)
        {
            _nextFreeImage.Value.Item2.sprite = Creator.chainSo.arrowPointingToNextPiece;
            _nextFreeImage.Value.Item2.gameObject.SetActive(true);
            _nextFreeImage.Value = (Piece.NotChosen, _nextFreeImage.Value.Item2);
            
            _nextFreeImage = _nextFreeImage.Next;
        }
        
        _nextFreeImage.Value.Item2.sprite = GetSprite(piece);
        _nextFreeImage.Value.Item2.color = _turnSystem.CurrentTurn() == LevPieceController.PieceColour.White
            ? Creator.piecesSo.whiteColor 
            : Creator.piecesSo.blackColor;
        _nextFreeImage.Value.Item2.gameObject.SetActive(true);
        _nextFreeImage.Value = (piece, _nextFreeImage.Value.Item2);

        _nextFreeImage = _nextFreeImage.Next;
    }

    private void ResetPosition()
    {
        _chainParent.localPosition = _chainParentInitialPos;
        _chainParentNewPos = _chainParentInitialPos;
        foreach ((Piece, Image) chainPiecesImage in _chainPiecesImages)
        {
            chainPiecesImage.Item2.color = new Color(1,1,1, 1);
        }
    }

    private void ClearChain()
    {
        foreach ((Piece, Image) capturedPiecesImage in _chainPiecesImages)
        {
            capturedPiecesImage.Item2.gameObject.SetActive(false);
        }
        _nextFreeImage = _chainPiecesImages.First;
    }

    public void HighlightNextPiece()
    {
        _chainParentNewPos += new Vector3(-260, 0, 0);
    }

    public void PawnPromoted(int index, Piece promotedPiece)
    {
        LinkedListNode<(Piece, Image)> temp = _chainPiecesImages.First;
        int tempIndex = 0;
        while (temp != null)
        {
            if (index == tempIndex)
            {
                (Piece, Image) value = temp.Value;
                value.Item1 = promotedPiece;
                value.Item2.sprite = GetSprite(promotedPiece);
                temp.Value = value;
                break;
            }

            tempIndex++;
            temp = temp.Next;
        }
    }

    public void PieceSandwiched(int index, Piece pieceToSandwich)
    {
        int tempIndex = 0;
        int numOfImagesActive = GetNumOfImagesActive();

        LinkedListNode<(Piece, Image)> temp = _chainPiecesImages.First;
        
        LinkedList<Piece> piecesInOrder = new();
        
        while (temp is not null && tempIndex <= numOfImagesActive)
        {
            if (index == tempIndex)
            {
                piecesInOrder.AddLast(pieceToSandwich);
            }
            else
            {
                if (temp.Value.Item1 != Piece.NotChosen)
                {
                    piecesInOrder.AddLast(temp.Value.Item1);
                }
                temp = temp.Next;
            }
            
            tempIndex++;
        }
        
        LinkedListNode<Piece> piecesInOrderTemp = piecesInOrder.First;
        _nextFreeImage = _chainPiecesImages.First;
        ShowNewPiece(piecesInOrderTemp.Value, true);
        piecesInOrderTemp = piecesInOrderTemp.Next;
        while (piecesInOrderTemp is not null)
        {
            ShowNewPiece(piecesInOrderTemp.Value);
            
            piecesInOrderTemp = piecesInOrderTemp.Next;
        }
    }

    private int GetNumOfImagesActive()
    {
        int count = 0;
        foreach ((Piece, Image) chainPiecesImage in _chainPiecesImages)
        {
            if (chainPiecesImage.Item2.gameObject.activeSelf)
                count++;
        }
        return count;
    }

    private Sprite GetSprite(Piece piece)
    {
        switch (piece)
        {
            case Piece.Pawn:
                return Creator.piecesSo.pawn;
            case Piece.Rook:
                return Creator.piecesSo.rook;
            case Piece.Knight:
                return Creator.piecesSo.knight;
            case Piece.Bishop:
                return Creator.piecesSo.bishop;
            case Piece.Queen:
                return Creator.piecesSo.queen;
            case Piece.King:
                return Creator.piecesSo.king;
            case Piece.NotChosen:
                return Creator.chainSo.arrowPointingToNextPiece;
        }
        
        //Given the logic of the code we should never get here but have to add something
        return null;
    }

    private void UpdateMovesRemainingText(int movesRemaining)
    {
        _movesRemainingText.gameObject.SetActive(false);
        _movesRemainingText.text = movesRemaining == 0 ? ". . ." : $"{movesRemaining}";
    }
    
    private void SceneManager_SceneUnloaded(Scene scene)
    {
        //Main menu scene should ALWAYS have 0 build index
        if (scene.buildIndex == 0)
        {
            Show();
        }
    }

    private void Show()
    {
        _guiTopChain.gameObject.SetActive(true);
    }

    private void Hide()
    {
        _guiTopChain.gameObject.SetActive(false);
    }

    public override void Clean()
    {
        SceneManager.sceneUnloaded -= SceneManager_SceneUnloaded;
    }
}
