using System.Collections.Generic;
using UnityEngine;

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

    public AllTagNames leftSidePanelOpen => _leftSidePanelOpen;
    public AllTagNames rightSidePanelOpen => _rightSidePanelOpen;
    
    private AllTagNames _leftSidePanelOpen;
    private AllTagNames _rightSidePanelOpen;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        foreach (Panel panel in _uiPanels)
        {
            panel.UIPanel.GameStart(creator);
            panel.UIPanel.Create(panel.TagName);
        }
    }

    public override void GameUpdate(float dt)
    {
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
                panel.UIPanel.Show();
            }
            else
            {
                panel.UIPanel.Hide();
            }
        }
        
        _leftSidePanelOpen = uiTag;
    }
    
    public void HideLeftSideUI()
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
                panel.UIPanel.Show();
            }
            else
            {
                panel.UIPanel.Hide();
            }
        }

        _rightSidePanelOpen = uiTag;
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
}
