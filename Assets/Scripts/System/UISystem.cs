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
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        foreach (Panel panel in _uiPanels)
        {
            panel.UIPanel.GameStart(creator);
            panel.UIPanel.Create(panel.TagName);
        }
    }
    
    public virtual void ShowUI(AllTagNames uiTag)
    {
        
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
