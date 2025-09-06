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

    public BlackSystem blackSystem => _blackSystem;
    public WhiteSystem whiteSystem => _whiteSystem;
    
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

    public override void GameUpdate(float dt)
    {
        Creator.statsTime += dt;
        
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
        if (samePoint)
        {
            HideTapPoint();
        }
        else
        {
            ShowTapPoint(gridPoint);
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
