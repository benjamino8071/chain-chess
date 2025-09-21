using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
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
    
    private float _mousePosYLastFrame;
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

        _pivot = Creator.GetChildComponentByName<Transform>(_panel.gameObject, AllTagNames.Pivot);
        
        int chainImagesAmount = 130;
        float yPos = 0;
        for (int i = 0; i < chainImagesAmount; i++)
        {
            GameObject chainPieceImageGo = Creator.InstantiateGameObjectWithParent(Creator.imagePrefab, _pivot);
            Image image = chainPieceImageGo.GetComponent<Image>();
            
            //Every odd image will be the arrow, which we want to be a smaller than the piece images
            if (i % 2 != 0)
            {
                chainPieceImageGo.transform.localScale = new(0.7f, 0.7f);
            }
            
            chainPieceImageGo.transform.localPosition = new(0, yPos, 0);
            yPos -= image.rectTransform.sizeDelta.y;
            
            _chainPieceImages.Add(new()
            {
                piece = Piece.NotChosen,
                image = image
            });
            
            chainPieceImageGo.SetActive(false);
        }

        _movesRemainingText = Creator.GetChildComponentByName<TextMeshProUGUI>(_panel.gameObject, AllTagNames.MovesRemaining);
        
        _chainParentInitialPos = _pivot.localPosition;
        _chainParentNewPos = _pivot.localPosition;
        
        _mousePosYLastFrame = Input.mousePosition.y;
    }
    
    public override void GameUpdate(float dt)
    {
        bool overPanel = false;
        
        List<RaycastResult> objectsUnderMouse = _uiSystem.objectsUnderMouse;
        foreach (RaycastResult objectUnderMouse in objectsUnderMouse)
        {
            if (objectUnderMouse.gameObject == _panel.gameObject)
            {
                overPanel = true;
                break;
            }
        }

        Vector2 scrollWheelValue = Creator.inputSo.scrollWheel.action.ReadValue<Vector2>();
        
        if (overPanel && Creator.inputSo.leftMouseButton.action.IsPressed())
        {
            float3 mousePos = Input.mousePosition;
            float mousePosYChange = mousePos.y - _mousePosYLastFrame;
            float3 chainParentLocalPos = _pivot.localPosition;
            chainParentLocalPos.y += mousePosYChange;
            _pivot.localPosition = chainParentLocalPos;
            
            _mouseOffTimer = 1;
        }
        else if (overPanel && scrollWheelValue.y != 0)
        {
            float positionChange = scrollWheelValue.y * Creator.inputSo.scrollPositionChange;
            float3 chainParentLocalPos = _pivot.localPosition;
            chainParentLocalPos.y += positionChange;
            _pivot.localPosition = chainParentLocalPos;
            
            _mouseOffTimer = 1;
        }
        else if(_mouseOffTimer > 0)
        {
            _mouseOffTimer -= dt;
        }
        else if(_chainParentNewPos != _pivot.localPosition)
        {
            Vector3 lerpPos = math.lerp(_pivot.localPosition, _chainParentNewPos, dt * Creator.chainSo.addPieceLerpSpeed);
            _pivot.localPosition = lerpPos;
        }
        
        _mousePosYLastFrame = Input.mousePosition.y;
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
        //For every other piece we first want to add an arrow indicating the order for the chain
        if (!isFirstPiece)
        {
            _chainPieceImages[_nextFreeIndex].image.sprite = Creator.chainSo.arrowPointingToNextPiece;
            _chainPieceImages[_nextFreeIndex].image.color = Creator.piecesSo.whiteColor;
            _chainPieceImages[_nextFreeIndex].image.rectTransform.sizeDelta = new(50, 50);
            _chainPieceImages[_nextFreeIndex].image.gameObject.SetActive(true);
            _chainPieceImages[_nextFreeIndex].piece = Piece.NotChosen;

            _nextFreeIndex++;
        }

        //Every other image is an arrow
        movesUsed *= 2;
        UpdateAlphaValue(0.1f, movesUsed);
        
        _chainPieceImages[_nextFreeIndex].image.sprite = GetSprite(piece);
        _chainPieceImages[_nextFreeIndex].image.color = Creator.piecesSo.whiteColor;
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
        _chainParentNewPos.y = 150 * pieceController.movesUsed;
        
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
