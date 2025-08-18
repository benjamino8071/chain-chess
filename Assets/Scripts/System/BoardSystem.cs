using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BoardSystem : Dependency
{
    private WhiteSystem _whiteSystem;
    private BlackSystem _blackSystem;
    private ChainUISystem _chainUISystem;
    private TurnSystem _turnSystem;
    private SettingsUISystem _settingsUISystem;
    private LevelSelectUISystem _levelSelectUISystem;
    private EndGameSystem _endGameSystem;

    public SideSystem activeSideSystem =>_turnSystem.CurrentTurn() == PieceColour.White 
        ? _whiteSystem 
        : _blackSystem;
    public SideSystem inactiveSideSystem =>_turnSystem.CurrentTurn() == PieceColour.White 
        ? _blackSystem 
        : _whiteSystem;
    
    private List<float3> _validTiles;
    
    private Transform _tapPoint;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _whiteSystem = creator.GetDependency<WhiteSystem>();
        _blackSystem = creator.GetDependency<BlackSystem>();
        _chainUISystem = creator.GetDependency<ChainUISystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _settingsUISystem = creator.GetDependency<SettingsUISystem>();
        _levelSelectUISystem = creator.GetDependency<LevelSelectUISystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();

        int validTilesCapacity = 64;
        _validTiles = new(validTilesCapacity);
        
        for (int y = 1; y < 9; y++)
        {
            for (int x = 1; x < 9; x++)
            {
                _validTiles.Add(new float3(x, y, 0));
            }
        }
        
        GameObject tapPoint = Creator.InstantiateGameObject(Creator.selectedBackgroundPrefab, float3.zero, quaternion.identity);
        _tapPoint = tapPoint.transform;
        HideTapPoint();
    }

    public override void GameEarlyUpdate(float dt)
    {
        /*float3 gridPoint = GetGridPointNearMouse();
        
        if (!Creator.inputSo.leftMouseButton.action.WasPerformedThisFrame()
            || _settingsUISystem.IsShowing
            || _levelSelectUISystem.IsShowing
            || _endGameSystem.isEndGame
            || !IsPositionValid(gridPoint))
        {
            return;
        }

        Creator.boardSo.hideMainMenuTrigger = true;
        
        bool samePoint = math.distance(gridPoint, _tapPoint.position) < 0.001f;
        UpdateTapPoint(gridPoint, samePoint);
        UpdateChain(gridPoint, samePoint);
        UpdateSelectedPiece(gridPoint, samePoint);*/
    }

    public override void GameUpdate(float dt)
    {
        float3 gridPoint = GetGridPointNearMouse();
        
        if (!Creator.inputSo.leftMouseButton.action.WasPerformedThisFrame()
            || _settingsUISystem.IsShowing
            || _levelSelectUISystem.IsShowing
            || _endGameSystem.isEndGame
            || !IsPositionValid(gridPoint))
        {
            return;
        }

        Creator.boardSo.hideMainMenuTrigger = true;
        
        bool samePoint = math.distance(gridPoint, _tapPoint.position) < 0.001f;
        UpdateTapPoint(gridPoint, samePoint);
        UpdateChain(gridPoint, samePoint);
        UpdateSelectedPiece(gridPoint, samePoint);
        
        Creator.statsTime += dt;
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
        
        if (_whiteSystem.TryGetAllyPieceAtPosition(gridPoint, out PieceController whitePieceController) 
            && !samePoint
            && !choseValidMovePoint)
        {
            _chainUISystem.ShowChain(whitePieceController);
        }
        else if (_blackSystem.TryGetAllyPieceAtPosition(gridPoint, out PieceController blackPieceController) 
                 && !samePoint
                 && !choseValidMovePoint)
        {
            _chainUISystem.ShowChain(blackPieceController);
        }
        else if(!choseValidMovePoint)
        {
            _chainUISystem.HideChain();
        }
    }

    private void UpdateSelectedPiece(float3 gridPoint, bool samePoint)
    {
        //Don't want to do anything if a valid move point has been chosen
        bool choseValidMovePoint = false;
        bool chosenPieceMoving = false;
        if (activeSideSystem.pieceControllerSelected is { } pieceControllerSelected)
        {
            choseValidMovePoint = pieceControllerSelected.GetAllValidMovesOfCurrentPiece().Contains(gridPoint);
            chosenPieceMoving = pieceControllerSelected.state == PieceController.States.Moving;
        }
        
        if (activeSideSystem.controlledBy != ControlledBy.Player 
            || choseValidMovePoint
            || chosenPieceMoving
            || activeSideSystem.frozen)
        {
            return;
        }

        if (activeSideSystem.TryGetAllyPieceAtPosition(gridPoint, out PieceController selectedPiece) && !samePoint)
        {
            activeSideSystem.SelectPiece(selectedPiece);
        }
        else
        {
            activeSideSystem.DeselectPiece();
        }
    }

    public bool IsPositionValid(float3 tilePos)
    {
        foreach (float3 validTile in _validTiles)
        {
            if (math.distance(validTile, tilePos) <= 0.0001f)
            {
                return true;
            }
        }
        
        return false;
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
    public bool TryCaptureEnemyPiece(Vector3 piecePos, PieceColour enemyColour, PieceController pieceUsed)
    {
        if (enemyColour == PieceColour.White 
            && _whiteSystem.TryGetAllyPieceAtPosition(piecePos, out PieceController whitePieceCont))
        {
            pieceUsed.AddCapturedPiece(whitePieceCont.capturedPieces[0]);
            _chainUISystem.ShowChain(pieceUsed);
            return _whiteSystem.PieceCaptured(whitePieceCont, pieceUsed);
        }
        if (enemyColour == PieceColour.Black 
            && _blackSystem.TryGetAllyPieceAtPosition(piecePos, out PieceController blackPieceCont))
        {
            pieceUsed.AddCapturedPiece(blackPieceCont.capturedPieces[0]);
            _chainUISystem.ShowChain(pieceUsed);
            return _blackSystem.PieceCaptured(blackPieceCont, pieceUsed);
        }
        
        return false;
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
        
        float3 screenToWorldPoint = Creator.mainCam.ScreenToWorldPoint(screenPos);

        float x = (int)screenToWorldPoint.x;
        float y = (int)screenToWorldPoint.y;

        return new(x, y, 0);
    }

    public float3 GetFloatPointOnBoard()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = 10;
        
        float3 screenToWorldPoint = Creator.mainCam.ScreenToWorldPoint(screenPos);

        screenToWorldPoint.y = math.clamp(screenToWorldPoint.y, Creator.boardSo.minY, Creator.boardSo.maxY);

        return screenToWorldPoint;
    }
}
