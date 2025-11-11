using System.Collections.Generic;
using Michsky.MUIP;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIChain : UIPanel
{
    private TurnSystem _turnSystem;
    
    private RectTransform _pivot;
    
    private Vector2 _chainParentInitialPos;
    private Vector2 _chainParentNewPos;

    private readonly List<Image> _chainPieceImages = new ();
    private int _nextFreeIndex;
    
    private float _mouseOffTimer;

    private bool _move;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _pivot = Creator.GetChildComponentByName<RectTransform>(_panel.gameObject, AllTagNames.Pivot);
        
        ButtonManager resetButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonReset);
        resetButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();

            _turnSystem.ReloadCurrentLevel();
        });
        
        int chainImagesAmount = 130;
        for (int i = 0; i < chainImagesAmount; i++)
        {
            GameObject chainPieceImageGo = Creator.InstantiateGameObjectWithParent(Creator.imagePrefab, _pivot);
            Image image = chainPieceImageGo.GetComponent<Image>();
            
            //Every odd image will be the arrow, which we want to be a smaller than the piece images
            if (i % 2 != 0)
            {
                RectTransform rectTransform = chainPieceImageGo.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new(37.5f,37.5f);
            }
            else
            {
                RectTransform rectTransform = chainPieceImageGo.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new(75,75);
            }
            
            _chainPieceImages.Add(image);
            
            chainPieceImageGo.SetActive(false);
        }
        
        _chainParentInitialPos = _pivot.anchoredPosition;
        _chainParentNewPos = _pivot.anchoredPosition;
    }
    
    public override void GameUpdate(float dt)
    {
        if (!_panel.gameObject.activeSelf)
        {
            return;
        }
        
        bool overPanel = false;
        
        List<RaycastResult> objectsUnderMouse = _parentCanvas.objectsUnderMouse;
        foreach (RaycastResult objectUnderMouse in objectsUnderMouse)
        {
            if (objectUnderMouse.gameObject == _parentCanvas.rightTopBackground.gameObject)
            {
                overPanel = true;
                break;
            }
        }

        bool input = Creator.inputSo.leftMouseButton.action.IsPressed() ||
                     Creator.inputSo.scrollWheel.action.WasPerformedThisFrame();
        if (overPanel && input)
        {
            _mouseOffTimer = 1;
            return;
        }
        
        if (_mouseOffTimer > 0)
        {
            _mouseOffTimer -= dt;
            if (_mouseOffTimer > 0)
            {
                return;
            }
        }
        
        if(_chainParentNewPos != _pivot.anchoredPosition)
        {
            Vector2 lerpPos = math.lerp(_pivot.anchoredPosition, _chainParentNewPos, dt * Creator.chainSo.addPieceLerpSpeed);
            _pivot.anchoredPosition = lerpPos;
        }
    }

    public void ShowChain(PlayerController pieceController)
    {
        ResetChain();
        
        for (int i = 0; i < pieceController.capturedPieces.Count; i++)
        {
            ShowNewPiece(pieceController.capturedPieces[i], pieceController.movesUsed, i == 0);
        }
        
        HighlightNextPiece(pieceController);
    }
    
    public void AddToChain(AIController capturedPiece, int movesUsed, int playerPieceCaptureCount)
    {
        ShowNewPiece(capturedPiece.piece, movesUsed);
    }
    
    private void ShowNewPiece(Piece piece, int movesUsed, bool isFirstPiece = false)
    {
        //For every other piece we first want to add an arrow indicating the order for the chain
        if (!isFirstPiece)
        {
            _chainPieceImages[_nextFreeIndex].sprite = _parentCanvas.canvasType == AllTagNames.LandscapeMode 
                ? Creator.chainSo.landscapeArrowSprite : Creator.chainSo.portraitArrowSprite;
            _chainPieceImages[_nextFreeIndex].color = Creator.piecesSo.whiteColor;
            _chainPieceImages[_nextFreeIndex].gameObject.SetActive(true);
            
            _nextFreeIndex++;
        }

        //Every other image is an arrow
        movesUsed *= 2;
        UpdateAlphaValue(0.1f, movesUsed);
        
        _chainPieceImages[_nextFreeIndex].sprite = GetSprite(piece);
        _chainPieceImages[_nextFreeIndex].color = Creator.piecesSo.whiteColor;
        _chainPieceImages[_nextFreeIndex].gameObject.SetActive(true);

        _nextFreeIndex++;
    }
    
    private void ResetChain()
    {
        foreach (Image chainPiecesImage in _chainPieceImages)
        {
            chainPiecesImage.color = new Color(1,1,1, 1);
        }
        
        foreach (Image capturedPiecesImage in _chainPieceImages)
        {
            capturedPiecesImage.gameObject.SetActive(false);
            Color imageColor = capturedPiecesImage.color;
            imageColor.a = 1f;
            capturedPiecesImage.color = imageColor;
        }
        
        _pivot.anchoredPosition = _chainParentInitialPos;
        _chainParentNewPos = _chainParentInitialPos;
        
        _nextFreeIndex = 0;
    }

    private void UpdateAlphaValue(float a, int amountToChange)
    {
        for (int i = 0; i < amountToChange; i++)
        {
            Color imageColor = _chainPieceImages[i].color;
            imageColor.a = a;
            _chainPieceImages[i].color = imageColor;
        }
    }

    public void HighlightNextPiece(PlayerController pieceController)
    {
        float newPosition = (75 + 37.5f) * pieceController.movesUsed;

        switch (_parentCanvas.canvasType)
        {
            case AllTagNames.LandscapeMode:
            {
                _chainParentNewPos.y = newPosition;
                break;
            }
            case AllTagNames.PortraitMode:
            {
                _chainParentNewPos.x = -newPosition;
                break;
            }
        }
        
        
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
                return Creator.chainSo.landscapeArrowSprite;
        }
        
        //Given the logic of the code we should never get here but have to add something
        return null;
    }
}
