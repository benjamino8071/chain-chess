using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChainUISystem : Dependency
{
    private BoardSystem _boardSystem;

    private PieceController _cachedPieceController;
    
    private Transform _guiTopChain;
    
    private Transform _chainParent;

    private TextMeshProUGUI _movesRemainingText;
    
    private Vector3 _chainParentInitialPos;
    private Vector3 _chainParentNewPos;

    private class PieceImage
    {
        public Piece piece;
        public Image image;
    }

    private List<PieceImage> _chainPieceImages = new ();
    private int _nextFreeIndex;
    
    private float _mousePosXLastFrame = -1;
    private float _mouseOffTimer;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _boardSystem = creator.GetDependency<BoardSystem>();

        _guiTopChain = creator.GetFirstObjectWithName(AllTagNames.GUITopChain);
        
        _chainParent = creator.GetChildObjectByName(_guiTopChain.gameObject, AllTagNames.ChainParent);
        
        Image[] chainPieceImages = _chainParent.GetComponentsInChildren<Image>();
        foreach (Image chainPieceImage in chainPieceImages)
        {
            _chainPieceImages.Add(new()
            {
                piece = Piece.NotChosen,
                image = chainPieceImage
            });
            chainPieceImage.gameObject.SetActive(false);
        }

        Transform movesRemainingText = creator.GetFirstObjectWithName(AllTagNames.MovesRemaining);
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
        if (ProcessSlider(dt) || _chainParentNewPos == _chainParent.localPosition)
        {
            return;
        }
        
        Vector3 lerpPos = math.lerp(_chainParent.localPosition, _chainParentNewPos, dt * Creator.chainSo.addPieceLerpSpeed);
        _chainParent.localPosition = lerpPos;
    }

    private bool ProcessSlider(float dt)
    {
        if (Creator.inputSo._leftMouseButton.action.IsPressed() && (_boardSystem.GetGridPointNearMouse().y > Creator.boardSo.maxY || _mousePosXLastFrame > 0))
        {
            float3 mousePos = Input.mousePosition;
            if (_mousePosXLastFrame > 0)
            {
                float mousePosXChange = mousePos.x - _mousePosXLastFrame;
                float3 chainParentLocalPos = _chainParent.localPosition;
                chainParentLocalPos.x += mousePosXChange;
                _chainParent.localPosition = chainParentLocalPos;
            }
            _mousePosXLastFrame = mousePos.x;
            _mouseOffTimer = 1f;
            return true;
        }
        else if (_mouseOffTimer > 0)
        {
            _mouseOffTimer -= dt;
            _mousePosXLastFrame = -1;
            return _mouseOffTimer > 0;
        }
        _mousePosXLastFrame = -1;
        return false;
    }

    public void ShowChain(PieceController pieceController)
    {
        ResetChain();
        
        for (int i = 0; i < pieceController.capturedPieces.Count; i++)
        {
            ShowNewPiece(pieceController.capturedPieces[i], pieceController.pieceColour, pieceController.movesUsed,i == 0);
        }
        
        _cachedPieceController = pieceController;
        HighlightNextPiece(pieceController);

        UpdateMovesRemainingText(pieceController.capturedPieces.Count);
    }

    //Public function 'hide' just has a better name than 'reset'
    public void HideChain()
    {
        ResetChain();
    }

    private void ResetChain()
    {
        foreach (PieceImage chainPiecesImage in _chainPieceImages)
        {
            chainPiecesImage.image.color = new Color(1,1,1, 1);
        }
        
        foreach (PieceImage capturedPiecesImage in _chainPieceImages)
        {
            capturedPiecesImage.image.gameObject.SetActive(false);
            Color imageColor = capturedPiecesImage.image.color;
            imageColor.a = 1f;
            capturedPiecesImage.image.color = imageColor;
        }
        
        _chainParent.localPosition = _chainParentInitialPos;
        _chainParentNewPos = _chainParentInitialPos;
        
        _nextFreeIndex = 0;
    }
    
    public void ShowNewPiece(Piece piece, PieceColour pieceColour, int movesUsed, bool isFirstPiece = false)
    {
        Color pieceColor = pieceColour == PieceColour.White
            ? Creator.piecesSo.whiteColor 
            : Creator.piecesSo.blackColor;
        
        //For every other piece we first want to add an arrow indicating the order for the chain
        if (!isFirstPiece)
        {
            _chainPieceImages[_nextFreeIndex].image.sprite = Creator.chainSo.arrowPointingToNextPiece;
            _chainPieceImages[_nextFreeIndex].image.color = pieceColor;
            _chainPieceImages[_nextFreeIndex].image.rectTransform.sizeDelta = new(50, 50);
            _chainPieceImages[_nextFreeIndex].image.gameObject.SetActive(true);
            _chainPieceImages[_nextFreeIndex].piece = Piece.NotChosen;

            _nextFreeIndex++;
        }

        //Every other image is an arrow
        movesUsed *= 2;
        UpdateAlphaValue(0.1f, movesUsed);
        
        _chainPieceImages[_nextFreeIndex].image.sprite = GetSprite(piece);
        _chainPieceImages[_nextFreeIndex].image.color = pieceColor;
        _chainPieceImages[_nextFreeIndex].image.rectTransform.sizeDelta = new(100, 100);
        _chainPieceImages[_nextFreeIndex].image.gameObject.SetActive(true);
        _chainPieceImages[_nextFreeIndex].piece  = piece;

        _nextFreeIndex++;
    }

    private void UpdateAlphaValue(float a, int amountToChange)
    {
        for (int i = 0; i < amountToChange; i++)
        {
            Color imageColor = _chainPieceImages[i].image.color;
            imageColor.a = a;
            _chainPieceImages[i].image.color = imageColor;
        }
    }

    public void HighlightNextPiece(PieceController pieceController)
    {
        if (pieceController != _cachedPieceController)
        {
            return;
        }
        
        _chainParentNewPos.x = -210 * pieceController.movesUsed;
        
        UpdateAlphaValue(0.1f, pieceController.movesUsed * 2);
    }

    public void PawnPromoted(int index, Piece promotedPiece)
    {
        int tempIndex = 0;
        while (tempIndex < _chainPieceImages.Count)
        {
            PieceImage pieceImage = _chainPieceImages[tempIndex];
            if (index == tempIndex)
            {
                pieceImage.piece = promotedPiece;
                pieceImage.image.sprite = GetSprite(promotedPiece);
                break;
            }

            tempIndex++;
        }
    }

    public void PieceSandwiched(int index, Piece pieceToSandwich)
    {
        //On reflection, this kind of goes against the idea of the chain
        //May confuse players
        /*int tempIndex = 0;
        int numOfImagesActive = GetNumOfImagesActive();
        
        List<Piece> piecesInOrder = new();
        
        while (tempIndex < _chainPieceImages.Count && tempIndex <= numOfImagesActive)
        {
            PieceImage pieceImage = _chainPieceImages[tempIndex];
            if (index == tempIndex)
            {
                piecesInOrder.Add(pieceToSandwich);
            }
            else if (pieceImage.piece != Piece.NotChosen)
            {
                piecesInOrder.Add(pieceImage.piece);
            }
            
            tempIndex++;
        }

        tempIndex = 0;
        while (tempIndex < _chainPieceImages.Count)
        {
            PieceImage pieceImage = _chainPieceImages[tempIndex];
            ShowNewPiece(pieceImage.piece, 0, tempIndex == 0);

            tempIndex++;
        }*/
    }

    private int GetNumOfImagesActive()
    {
        int count = 0;
        foreach (PieceImage chainPiecesImage in _chainPieceImages)
        {
            if (chainPiecesImage.image.gameObject.activeSelf)
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

    public override void Destroy()
    {
        SceneManager.sceneUnloaded -= SceneManager_SceneUnloaded;
    }
}
