using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class LevBoardSystem : LevDependency
{
    private LevWhiteSystem _whiteSystem;
    private LevBlackSystem _blackSystem;
    private LevChainUISystem _chainUISystem;
    private LevValidMovesSystem _validMovesSystem;
    private LevTurnSystem _turnSystem;
    private LevPauseUISystem _pauseUISystem;
    private LevEndGameSystem _endGameSystem;

    public LevSideSystem activeSideSystem =>_turnSystem.CurrentTurn() == PieceColour.White 
        ? _whiteSystem 
        : _blackSystem;
    
    private Dictionary<Vector3, TileController> _validTiles = new();

    private Dictionary<Vector3, List<Vector3>> _connectedTiles = new();
    
    private Transform _tapPoint;
    
    private Vector3 _highlightedPosition = Vector3.zero;
    
    private Camera _camera;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _whiteSystem = levCreator.GetDependency<LevWhiteSystem>();
        _blackSystem = levCreator.GetDependency<LevBlackSystem>();
        _chainUISystem = levCreator.GetDependency<LevChainUISystem>();
        _validMovesSystem = levCreator.GetDependency<LevValidMovesSystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        _pauseUISystem = levCreator.GetDependency<LevPauseUISystem>();
        _endGameSystem = levCreator.GetDependency<LevEndGameSystem>();

        _camera = Camera.main;
        if (!_camera)
        {
            Debug.LogError("CAN'T FIND CAMERA");
            return;
        }
        
        List<Transform> tilesAlreadyPlaced = levCreator.GetObjectsByName(AllTagNames.Tile);
        foreach (Transform tileChild in tilesAlreadyPlaced)
        {
            TileController tileController = new TileController();
            tileController.SetTile(tileChild.transform, Creator);
                
            _validTiles.Add(tileChild.transform.position, tileController);
        }
        
        //Go through every position, and find all nearby positions. We do this by checking if a nearby position is valid
        foreach (Vector3 positionInGrid in _validTiles.Keys)
        {
            List<Vector3> nearbyPositions = new();
            
            //Check all positions around grid
            for (float x = positionInGrid.x - 1; x <= positionInGrid.x + 1; x++)
            {
                for (float y = positionInGrid.y - 1; y <= positionInGrid.y + 1; y++)
                {
                    Vector3 positionToCheck = new Vector3(x, y);
                    if (IsPositionValid(positionToCheck) && positionToCheck != positionInGrid)
                    {
                        nearbyPositions.Add(positionToCheck);
                    }
                }
            }
            
            _connectedTiles.Add(positionInGrid, nearbyPositions);
        }
        
        GameObject tapPoint =
            Creator.InstantiateGameObject(Creator.selectedBackgroundPrefab, Vector3.zero, Quaternion.identity);
        _tapPoint = tapPoint.transform;
        HideTapPoint();
    }

    public override void GameEarlyUpdate(float dt)
    {
        Vector3 gridPoint = GetGridPointNearMouse();
        if (!Creator.inputSo._leftMouseButton.action.WasPerformedThisFrame()
            || _pauseUISystem.isShowing
            || _endGameSystem.isEndGame
            || !IsPositionValid(gridPoint))
        {
            return;
        }

        Creator.playerSystemSo.hideMainMenuTrigger = true;
        
        bool samePoint = gridPoint == _tapPoint.position;
        UpdateTapPoint(gridPoint, samePoint);
        UpdateChain(gridPoint, samePoint);
        UpdateSelectedPiece(gridPoint, samePoint);
    }

    public override void GameUpdate(float dt)
    {
        if (Creator.isPuzzle)
        {
            Creator.statsTime += dt;
        }
    }

    private void UpdateTapPoint(float3 gridPoint, bool samePoint)
    {
        if (samePoint)
        {
            HideTapPoint();
        }
        else
        {
            ShowTapPoint(gridPoint);
        }
    }

    private void UpdateChain(float3 gridPoint, bool samePoint)
    {
        //Don't want to do anything if a valid move point has been chosen
        bool choseValidMovePoint = activeSideSystem.pieceControllerSelected is { } pieceControllerSelected
                                   && pieceControllerSelected.GetAllValidMovesOfCurrentPiece().Contains(gridPoint);
        
        if (_whiteSystem.TryGetAllyPieceAtPosition(gridPoint, out LevPieceController whitePieceController) 
            && !samePoint
            && !choseValidMovePoint)
        {
            _chainUISystem.ShowChain(whitePieceController.capturedPieces, whitePieceController.pieceColour, whitePieceController.movesUsed);
        }
        else if (_blackSystem.TryGetAllyPieceAtPosition(gridPoint, out LevPieceController blackPieceController) 
                 && !samePoint
                 && !choseValidMovePoint)
        {
            _chainUISystem.ShowChain(blackPieceController.capturedPieces, blackPieceController.pieceColour, blackPieceController.movesUsed);
        }
        else if(!choseValidMovePoint)
        {
            _chainUISystem.HideChain();
        }
    }

    private void UpdateSelectedPiece(float3 gridPoint, bool samePoint)
    {
        //Don't want to do anything if a valid move point has been chosen
        bool choseValidMovePoint = activeSideSystem.pieceControllerSelected is { } pieceControllerSelected
                                   && pieceControllerSelected.GetAllValidMovesOfCurrentPiece().Contains(gridPoint);
        
        if (activeSideSystem.controlledBy != ControlledBy.Player 
            || choseValidMovePoint)
        {
            return;
        }

        if (activeSideSystem.TryGetAllyPieceAtPosition(gridPoint, out LevPieceController selectedPiece) && !samePoint)
        {
            activeSideSystem.SelectPiece(selectedPiece);
        }
        else
        {
            activeSideSystem.DeselectPiece();
        }
    }

    public bool IsPositionValid(Vector3 tilePos)
    {
        return _validTiles.ContainsKey(tilePos);
    }

    public bool IsAllyAtPosition(Vector3 piecePos, PieceColour allyColour)
    {
        List<Vector3> enemyPositions = allyColour == PieceColour.White 
            ? _whiteSystem.PiecePositions() 
            : _blackSystem.PiecePositions();

        return enemyPositions.Contains(piecePos);
    }

    public bool IsEnemyAtPosition(Vector3 piecePos, PieceColour enemyColour)
    {
        List<Vector3> enemyPositions = enemyColour == PieceColour.White 
            ? _whiteSystem.PiecePositions() 
            : _blackSystem.PiecePositions();
        
        return enemyPositions.Contains(piecePos);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="piecePos"></param>
    /// <param name="enemyColour"></param>
    /// <param name="pieceUsed"></param>
    /// <returns>True = captured all enemy pieces of pieceUsed</returns>
    public bool TryCaptureEnemyPiece(Vector3 piecePos, PieceColour enemyColour, LevPieceController pieceUsed)
    {
        if (enemyColour == PieceColour.White 
            && _whiteSystem.TryGetAllyPieceAtPosition(piecePos, out LevPieceController whitePieceCont))
        {
            pieceUsed.AddCapturedPiece(whitePieceCont.capturedPieces[0]);
            _chainUISystem.ShowChain(pieceUsed.capturedPieces, PieceColour.Black, pieceUsed.movesUsed);
            return _whiteSystem.PieceCaptured(whitePieceCont, pieceUsed.piecesCapturedInThisTurn, pieceUsed.movesUsed);
        }
        if (enemyColour == PieceColour.Black 
            && _blackSystem.TryGetAllyPieceAtPosition(piecePos, out LevPieceController blackPieceCont))
        {
            pieceUsed.AddCapturedPiece(blackPieceCont.capturedPieces[0]);
            _chainUISystem.ShowChain(pieceUsed.capturedPieces, PieceColour.White, pieceUsed.movesUsed);
            return _blackSystem.PieceCaptured(blackPieceCont, pieceUsed.piecesCapturedInThisTurn, pieceUsed.movesUsed);
        }
        
        return false;
    }

    public void PieceLocked(LevPieceController pieceController, PieceColour sideColour)
    {
        if (sideColour == PieceColour.White)
        {
            _whiteSystem.PieceLocked(pieceController);
        }
        else
        {
            _blackSystem.PieceLocked(pieceController);
        }
    }
    
    public void ShowTapPoint(float3 position)
    {
        _tapPoint.position = position;
        _tapPoint.gameObject.SetActive(true);
    }

    public void HideTapPoint()
    {
        _tapPoint.position = Vector3.zero;
        _tapPoint.gameObject.SetActive(false);
    }

    public float3 GetGridPointNearMouse()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = 10;
        
        float3 screenToWorldPoint = _camera.ScreenToWorldPoint(screenPos);

        float x = (int)screenToWorldPoint.x + 0.5f;
        float y = (int)screenToWorldPoint.y + 0.5f;

        return new(x, y, 0);
    }

    public float3 GetScreenToWorldPoint()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = 10;
        
        return _camera.ScreenToWorldPoint(screenPos);
    }
}
