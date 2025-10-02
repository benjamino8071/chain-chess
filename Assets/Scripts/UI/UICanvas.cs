using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICanvas : Dependency
{
    public Transform canvas => _canvas;
    
    private AllTagNames _canvasType;
    private Transform _canvas;
    
    private class Panel
    {
        public AllTagNames TagName;
        public UIPanel UIPanel;
    }

    private List<Panel> _leftBotPanels = new()
    {
        new()
        {
            TagName = AllTagNames.UIRulebook,
            UIPanel = new UIRulebook()
        },
        new()
        {
            TagName = AllTagNames.UISettings,
            UIPanel = new UISettings()
        },
        new()
        {
            TagName = AllTagNames.UISections,
            UIPanel = new UISections()
        },
    };
    private Panel _uiCurrentLevel = new()
    {
        TagName = AllTagNames.UICurrentLevel,
        UIPanel = new UICurrentLevelMenu()
    };
    private Panel _uiBasicMenu = new()
    {
        TagName = AllTagNames.UIBasicMenu,
        UIPanel = new UIBasicMenu()
    };

    private List<Panel> _rightTopPanels = new()
    {
        new()
        {
            TagName = AllTagNames.UIChain,
            UIPanel = new UIChain()
        },
        new()
        {
            TagName = AllTagNames.UILevelComplete,
            UIPanel = new UILevelComplete()
        },
        new()
        {
            TagName = AllTagNames.UIGameOver,
            UIPanel = new UIGameOver()
        },
        new()
        {
            TagName = AllTagNames.UILevels,
            UIPanel = new UILevels()
        },
    };

    public List<RaycastResult> objectsUnderMouse => _objectsUnderMouse;

    public AllTagNames leftBotSidePanelOpen => _leftBotSidePanelOpen;
    public AllTagNames rightTopSidePanelOpen => _rightTopSidePanelOpen;
    
    public Image leftBotBackground => _leftBotBackground;
    public Image rightTopBackground => _rightTopBackground;
    
    private AllTagNames _leftBotSidePanelOpen;
    private AllTagNames _rightTopSidePanelOpen;

    private List<RaycastResult> _objectsUnderMouse;
    
    private Image _leftBotBackground;
    private ScaleTween _leftBotBackgroundTween;
    private Image _rightTopBackground;
    private ScaleTween _rightTopBackgroundTween;
    
    public void AssignCanvas(AllTagNames canvasTypeTag)
    {
        _canvasType = canvasTypeTag;
    }
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _canvas = creator.GetFirstObjectWithName(_canvasType);
        
        _leftBotBackground = creator.GetChildComponentByName<Image>(_canvas.gameObject, AllTagNames.LeftBottomBackground);
        _leftBotBackgroundTween = _leftBotBackground.GetComponent<ScaleTween>();
        _rightTopBackground = creator.GetChildComponentByName<Image>(_canvas.gameObject, AllTagNames.RightTopBackground);
        _rightTopBackgroundTween = _rightTopBackground.GetComponent<ScaleTween>();

        foreach (Panel panel in _leftBotPanels)
        {
            panel.UIPanel.AssignCanvas(this);
            panel.UIPanel.GameStart(creator);
            panel.UIPanel.Create(panel.TagName);
        }
        _uiCurrentLevel.UIPanel.AssignCanvas(this);
        _uiCurrentLevel.UIPanel.GameStart(creator);
        _uiCurrentLevel.UIPanel.Create(_uiCurrentLevel.TagName);
        
        _uiBasicMenu.UIPanel.AssignCanvas(this);
        _uiBasicMenu.UIPanel.GameStart(creator);
        _uiBasicMenu.UIPanel.Create(_uiBasicMenu.TagName);

        
        foreach (Panel panel in _rightTopPanels)
        {
            panel.UIPanel.AssignCanvas(this);
            panel.UIPanel.GameStart(creator);
            panel.UIPanel.Create(panel.TagName);
        }
    }

    public override void GameUpdate(float dt)
    {
        _objectsUnderMouse = GetEventSystemRaycastResults();
        
        foreach (Panel panel in _leftBotPanels)
        {
            panel.UIPanel.GameUpdate(dt);
        }
        _uiCurrentLevel.UIPanel.GameUpdate(dt);
        _uiBasicMenu.UIPanel.GameUpdate(dt);
        
        foreach (Panel panel in _rightTopPanels)
        {
            panel.UIPanel.GameUpdate(dt);
        }
    }

    public void ShowLeftBotSideUI(AllTagNames uiTag)
    {
        foreach (Panel panel in _leftBotPanels)
        {
            if (panel.TagName == uiTag)
            {
                _leftBotBackgroundTween.PhaseIn();
                
                panel.UIPanel.Show();
            }
            else
            {
                panel.UIPanel.Hide();
            }
        }
        
        _leftBotSidePanelOpen = uiTag;
        _leftBotBackground.gameObject.SetActive(true);
    }
    
    public void HideLeftBotSideUI()
    {
        _leftBotBackgroundTween.PhaseOut();
        
        _leftBotSidePanelOpen = AllTagNames.None;
    }

    public void HideLeftBotSideUINoTween()
    {
        foreach (Panel panel in _leftBotPanels)
        {
            panel.UIPanel.Hide();
        }
        
        _leftBotSidePanelOpen = AllTagNames.None;
        _leftBotBackground.gameObject.SetActive(false);
    }
    
    public void ShowRightTopSideUI(AllTagNames uiTag)
    {
        foreach (Panel panel in _rightTopPanels)
        {
            if (panel.TagName == uiTag)
            {
                _rightTopBackgroundTween.PhaseIn();
                
                SetBackgroundColour(_rightTopBackground, uiTag);
                
                panel.UIPanel.Show();
            }
            else
            {
                panel.UIPanel.Hide();
            }
        }

        _rightTopSidePanelOpen = uiTag;
        _rightTopBackground.gameObject.SetActive(true);
    }
    
    public void HideRightTopSideUI()
    {
        foreach (Panel panel in _rightTopPanels)
        {
            panel.UIPanel.Hide();
        }
        
        _rightTopSidePanelOpen = AllTagNames.None;
        _rightTopBackground.gameObject.SetActive(false);
    }

    public T GetUI<T>() where T : UIPanel
    {
        foreach (Panel panel in _leftBotPanels)
        {
            if (panel.UIPanel is T)
            {
                return (T)panel.UIPanel;
            }
        }
        foreach (Panel panel in _rightTopPanels)
        {
            if (panel.UIPanel is T)
            {
                return (T)panel.UIPanel;
            }
        }
        if (_uiBasicMenu.UIPanel is T)
        {
            return (T)_uiBasicMenu.UIPanel;
        }
        if (_uiCurrentLevel.UIPanel is T)
        {
            return (T)_uiCurrentLevel.UIPanel;
        }
        
        Debug.LogError("Could not find UI Panel");
        return null;
    }
    
    //Gets all event system raycast results of current mouse or touch position.
    private static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
    
    private void SetBackgroundColour(Image background, AllTagNames uiTag)
    {
        switch (uiTag)
        {
            case AllTagNames.UILevelComplete:
            {
                background.color = Creator.miscUiSo.levelCompleteBackgroundColour;
                break;
            }
            case AllTagNames.UIGameOver:
            {
                background.color = Creator.miscUiSo.gameOverBackgroundColour;
                break;
            }
            default:
            {
                background.color = Creator.miscUiSo.normalBackgroundColour;
                break;
            }
        }
    }

    public void SetVisibility(bool show)
    {
        _canvas.gameObject.SetActive(show);
    }
}
