using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISystem : Dependency
{
    private UICanvas _landscapeCanvas;
    private UICanvas _portraitCanvas;

    private UICanvas _currentCanvas;

    public List<RaycastResult> objectsUnderMouse => _currentCanvas.objectsUnderMouse;

    public AllTagNames leftBotSidePanelOpen => _currentCanvas.leftBotSidePanelOpen;
    public AllTagNames rightTopSidePanelOpen => _currentCanvas.rightTopSidePanelOpen;
    
    public Image leftBotBackground => _currentCanvas.leftBotBackground;
    public Image rightTopBackground => _currentCanvas.rightTopBackground;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _landscapeCanvas = new();
        _landscapeCanvas.AssignCanvas(AllTagNames.LandscapeMode);
        _landscapeCanvas.GameStart(creator);
        _landscapeCanvas.SetVisibility(false);
        
        _portraitCanvas = new();
        _portraitCanvas.AssignCanvas(AllTagNames.PortraitMode);
        _portraitCanvas.GameStart(creator);
        _portraitCanvas.SetVisibility(false);
        
        UpdateCurrentCanvas();
        
        if (Creator.saveDataSo.isFirstTime)
        {
            ShowLeftBotSideUI(AllTagNames.UIRulebook);
        }
        else
        {
            HideLeftBotSideUINoTween();
        }
        
        //Turn system handles showing chain
    }
    
    public override void GameUpdate(float dt)
    {
        UpdateCurrentCanvas();
        
        _landscapeCanvas.GameUpdate(dt);
        _portraitCanvas.GameUpdate(dt);
    }

    private void UpdateCurrentCanvas()
    {
        float width = Screen.width;
        float height = Screen.height;
        
        float aspect = width / height;

        if (aspect < Creator.settingsSo.aspectRatioToChangeValue && _currentCanvas != _portraitCanvas)
        {
            _landscapeCanvas.SetVisibility(false);
            _portraitCanvas.SetVisibility(true);
            
            _currentCanvas = _portraitCanvas;
            Creator.mainCam.orthographicSize = Creator.settingsSo.portraitModeSettings.cameraSize;
        }
        else if (aspect >= Creator.settingsSo.aspectRatioToChangeValue && _currentCanvas != _landscapeCanvas)
        {
            _portraitCanvas.SetVisibility(false);
            _landscapeCanvas.SetVisibility(true);
            
            _currentCanvas = _landscapeCanvas;
            Creator.mainCam.orthographicSize = Creator.settingsSo.landscapeModeSettings.cameraSize;
        }
    }

    public void ShowLeftBotSideUI(AllTagNames uiTag)
    {
        _landscapeCanvas.ShowLeftBotSideUI(uiTag);
        _portraitCanvas.ShowLeftBotSideUI(uiTag);
    }
    
    private void HideLeftBotSideUINoTween()
    {
        _landscapeCanvas.HideLeftBotSideUINoTween();
        _portraitCanvas.HideLeftBotSideUINoTween();
    }
    
    public void HideLeftBotSideUI()
    {
        _landscapeCanvas.HideLeftBotSideUI();
        _portraitCanvas.HideLeftBotSideUI();
    }
    
    public void ShowRightTopSideUI(AllTagNames uiTag)
    {
        _landscapeCanvas.ShowRightTopSideUI(uiTag);
        _portraitCanvas.ShowRightTopSideUI(uiTag);
    }
    
    public void HideRightTopSideUI()
    {
        _landscapeCanvas.HideRightTopSideUI();
        _portraitCanvas.HideRightTopSideUI();
    }

    public List<T> GetUI<T>() where T : UIPanel
    {
        List<T> list = new(2)
        {
            _portraitCanvas.GetUI<T>(),
            _landscapeCanvas.GetUI<T>()
        };

        return list;
    }
}
