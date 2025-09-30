using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISystem : Dependency
{
    private class Panel
    {
        public AllTagNames TagName;
        public UIPanel UIPanel;
    }
    
    private List<Panel> _uiPanels = new()
    {
        new()
        {
            TagName = AllTagNames.UISettings,
            UIPanel = new UISettings()
        },
        new()
        {
            TagName = AllTagNames.UILevels,
            UIPanel = new UILevels()
        },
        new()
        {
            TagName = AllTagNames.UISections,
            UIPanel = new UISections()
        },
        new()
        {
            TagName = AllTagNames.UICurrentLevel,
            UIPanel = new UICurrentLevelMenu()
        },
        new()
        {
            TagName = AllTagNames.UIChain,
            UIPanel = new UIChain()
        },
        new()
        {
            TagName = AllTagNames.UIBasicMenu,
            UIPanel = new UIBasicMenu()
        },
        new()
        {
            TagName = AllTagNames.UIRulebook,
            UIPanel = new UIRulebook()
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
    };

    private List<AllTagNames> _leftSideTags = new()
    {
        AllTagNames.UIRulebook,
        AllTagNames.UISettings,
        AllTagNames.UISections,
    };
    
    private List<AllTagNames> _rightSideTags = new()
    {
        AllTagNames.UIChain,
        AllTagNames.UILevelComplete,
        AllTagNames.UIGameOver,
        AllTagNames.UILevels,
    };

    public List<RaycastResult> objectsUnderMouse => _objectsUnderMouse;

    public AllTagNames leftSidePanelOpen => _leftSidePanelOpen;
    public AllTagNames rightSidePanelOpen => _rightSidePanelOpen;
    
    public Image leftBackground => _leftBackground;
    public Image rightBackground => _rightBackground;
    
    private AllTagNames _leftSidePanelOpen;
    private AllTagNames _rightSidePanelOpen;

    private List<RaycastResult> _objectsUnderMouse;
    
    private Image _leftBackground;
    private ScaleTween _leftBackgroundTween;
    private Image _rightBackground;
    private ScaleTween _rightBackgroundTween;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _leftBackground = creator.GetFirstObjectWithName(AllTagNames.LeftBackground).GetComponent<Image>();
        _leftBackgroundTween = _leftBackground.GetComponent<ScaleTween>();
        _rightBackground = creator.GetFirstObjectWithName(AllTagNames.RightBackground).GetComponent<Image>();
        _rightBackgroundTween = _rightBackground.GetComponent<ScaleTween>();
        
        foreach (Panel panel in _uiPanels)
        {
            panel.UIPanel.GameStart(creator);
            panel.UIPanel.Create(panel.TagName);
        }
        
        if (Creator.saveDataSo.isFirstTime)
        {
            ShowLeftSideUI(AllTagNames.UIRulebook);
        }
        else
        {
            HideLeftSideUINoTween();
        }
    }

    public override void GameUpdate(float dt)
    {
        _objectsUnderMouse = GetEventSystemRaycastResults();
        
        foreach (Panel panel in _uiPanels)
        {
            panel.UIPanel.GameUpdate(dt);
        }
    }

    public void ShowLeftSideUI(AllTagNames uiTag)
    {
        foreach (Panel panel in _uiPanels)
        {
            if (!_leftSideTags.Contains(panel.TagName))
            {
                continue;
            }
            
            if (panel.TagName == uiTag)
            {
                _leftBackgroundTween.PhaseIn();
                
                panel.UIPanel.Show();
            }
            else
            {
                panel.UIPanel.Hide();
            }
        }
        
        _leftSidePanelOpen = uiTag;
        _leftBackground.gameObject.SetActive(true);
    }
    
    public void HideLeftSideUI()
    {
        _leftBackgroundTween.PhaseOut();
        
        _leftSidePanelOpen = AllTagNames.None;
    }

    private void HideLeftSideUINoTween()
    {
        foreach (Panel panel in _uiPanels)
        {
            if (!_leftSideTags.Contains(panel.TagName))
            {
                continue;
            }
            
            panel.UIPanel.Hide();
        }
        
        _leftSidePanelOpen = AllTagNames.None;
        _leftBackground.gameObject.SetActive(false);
    }
    
    public void ShowRightSideUI(AllTagNames uiTag)
    {
        foreach (Panel panel in _uiPanels)
        {
            if (!_rightSideTags.Contains(panel.TagName))
            {
                continue;
            }
            
            if (panel.TagName == uiTag)
            {
                _rightBackgroundTween.PhaseIn();
                
                SetBackgroundColour(_rightBackground, uiTag);
                
                panel.UIPanel.Show();
            }
            else
            {
                panel.UIPanel.Hide();
            }
        }

        _rightSidePanelOpen = uiTag;
        _rightBackground.gameObject.SetActive(true);
    }
    
    public void HideRightSideUI()
    {
        foreach (Panel panel in _uiPanels)
        {
            if (!_rightSideTags.Contains(panel.TagName))
            {
                continue;
            }
            
            panel.UIPanel.Hide();
        }
        
        _rightSidePanelOpen = AllTagNames.None;
        _rightBackground.gameObject.SetActive(false);
    }

    public T GetUI<T>() where T : UIPanel
    {
        foreach (Panel panel in _uiPanels)
        {
            if (panel.UIPanel is T)
            {
                return (T)panel.UIPanel;
            }
        }
        
        Debug.LogError("Could not find UI Panel");
        return null;
    }
    
    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
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
}
