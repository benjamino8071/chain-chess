using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIChain : UIPanel
{
    private BoardSystem _boardSystem;
    private AudioSystem _audioSystem;

    private Transform _pivot;

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

    private bool _move;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _boardSystem = creator.GetDependency<BoardSystem>();
        _audioSystem = creator.GetDependency<AudioSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        Transform uiChain = Creator.GetFirstObjectWithName(AllTagNames.UIChain);
        _pivot = Creator.GetChildObjectByName(uiChain.gameObject, AllTagNames.Pivot);

        int chainImagesAmount = 130;
        float xPos = 0;
        for (int i = 0; i < chainImagesAmount; i++)
        {
            GameObject chainPieceImage = Creator.InstantiateGameObjectWithParent(Creator.imagePrefab, _pivot);
            //Every odd image will be the arrow, which we want to be a smaller than the piece images
            if (i % 2 != 0)
            {
                chainPieceImage.transform.localScale = new(0.7f, 0.7f);
            }
            
            Image image = chainPieceImage.GetComponent<Image>();
            
            chainPieceImage.transform.localPosition = new(xPos, 0, 0);
            xPos += image.rectTransform.sizeDelta.x;
            
            _chainPieceImages.Add(new()
            {
                piece = Piece.NotChosen,
                image = image
            });
            
            chainPieceImage.SetActive(false);
        }

        Transform movesRemainingText = Creator.GetFirstObjectWithName(AllTagNames.MovesRemaining);
        _movesRemainingText = movesRemainingText.GetComponentInChildren<TextMeshProUGUI>();
        
        _chainParentInitialPos = _pivot.localPosition;
        _chainParentNewPos = _pivot.localPosition;
    }
    
    public override void GameUpdate(float dt)
    {
        if (ProcessSlider(dt) || _chainParentNewPos == _pivot.localPosition)
        {
            return;
        }
        
        Vector3 lerpPos = math.lerp(_pivot.localPosition, _chainParentNewPos, dt * Creator.chainSo.addPieceLerpSpeed);
        _pivot.localPosition = lerpPos;
    }
    
    private bool ProcessSlider(float dt)
    {
        if(Creator.inputSo.leftMouseButton.action.WasPressedThisFrame() && (_boardSystem.GetGridPointNearMouse().y < Creator.boardSo.minY || _mousePosXLastFrame > 0))
        {
            _move = true;
        }
        else if (Creator.inputSo.leftMouseButton.action.WasReleasedThisFrame())
        {
            _move = false;
        }
        
        if (_move)
        {
            float3 mousePos = Input.mousePosition;
            if (_mousePosXLastFrame > 0)
            {
                float mousePosXChange = mousePos.x - _mousePosXLastFrame;
                float3 chainParentLocalPos = _pivot.localPosition;
                chainParentLocalPos.x += mousePosXChange;
                _pivot.localPosition = chainParentLocalPos;
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

    public void ShowChain(PlayerController pieceController, bool setMovesRemainingText)
    {
        ResetChain();
        
        for (int i = 0; i < pieceController.capturedPieces.Count; i++)
        {
            ShowNewPiece(pieceController.capturedPieces[i], pieceController.movesUsed, i == 0);
        }
        
        HighlightNextPiece(pieceController);

        if (setMovesRemainingText)
        {
            UpdateMovesRemainingText(pieceController.capturedPieces.Count);
        }
    }
    
    public void AddToChain(AIController capturedPiece, int movesUsed, int playerPieceCaptureCount)
    {
        ShowNewPiece(capturedPiece.piece, movesUsed);
        
        UpdateMovesRemainingText(playerPieceCaptureCount);
    }
    
    private void ShowNewPiece(Piece piece, int movesUsed, bool isFirstPiece = false)
    {
        Color pieceColor = Creator.piecesSo.whiteColor;
        
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
        
        _pivot.localPosition = _chainParentInitialPos;
        _chainParentNewPos = _chainParentInitialPos;
        
        _nextFreeIndex = 0;
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

    public void HighlightNextPiece(PlayerController pieceController)
    {
        _chainParentNewPos.x = -150 * pieceController.movesUsed;
        
        UpdateAlphaValue(0.1f, pieceController.movesUsed * 2);
    }

    public void HideAllPieces()
    {
        UpdateAlphaValue(0.1f, _chainPieceImages.Count);
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

    private void UpdateMovesRemainingText(int movesInChain)
    {
        _movesRemainingText.text = movesInChain == 0 ? ". . ." : $"{movesInChain}";
    }
}
