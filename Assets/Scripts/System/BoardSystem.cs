using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BoardSystem : Dependency
{
    private WhiteSystem _whiteSystem;
    private BlackSystem _blackSystem;
    private TurnSystem _turnSystem;
    private UISystem _uiSystem;
    private EndGameSystem _endGameSystem;
    
    private List<float3> _validTiles;
    
    private Transform _tapPoint;
    
    private ColourFlicker _edgeColourFlicker;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _whiteSystem = creator.GetDependency<WhiteSystem>();
        _blackSystem = creator.GetDependency<BlackSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _uiSystem = creator.GetDependency<UISystem>();
        _endGameSystem = creator.GetDependency<EndGameSystem>();
        
        Transform edge = Creator.GetFirstObjectWithName(AllTagNames.Edge);
        _edgeColourFlicker = edge.GetComponent<ColourFlicker>();

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
        float3 gridPoint = GetGridPointNearMouse();
        
        if (!Creator.inputSo.leftMouseButton.action.WasPerformedThisFrame()
            || _endGameSystem.isEndGame
            || !IsPositionValid(gridPoint))
        {
            return;
        }

        if (_uiSystem.leftBotSidePanelOpen == AllTagNames.UISections)
        {
            _uiSystem.HideLeftBotSideUI();
        }

        if (_uiSystem.rightTopSidePanelOpen == AllTagNames.UILevels)
        {
            _uiSystem.ShowRightTopSideUI(AllTagNames.UIChain);
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
    
    public void FlickerEdge()
    {
        _edgeColourFlicker.ChangeColour();
    }
}
