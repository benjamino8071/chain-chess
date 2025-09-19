using UnityEngine;

public class UIPanel : Dependency
{
    public bool IsShowing => _panel.gameObject.activeSelf;
    
    protected Transform _panel;

    public virtual void Create(AllTagNames uiTag)
    {
        _panel = Creator.GetFirstObjectWithName(uiTag);
    }

    public virtual void Show()
    {
        _panel.gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        _panel.gameObject.SetActive(false);
    }
}
