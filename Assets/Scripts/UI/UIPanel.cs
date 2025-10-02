using UnityEngine;

public class UIPanel : Dependency
{
    protected UISystem _uiSystem;
    protected AudioSystem _audioSystem;
    
    public bool IsShowing => _panel.gameObject.activeSelf;
    
    protected Transform _panel;

    protected UICanvas _parentCanvas;

    public void AssignCanvas(UICanvas canvas)
    {
        _parentCanvas = canvas;
    }
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _uiSystem = creator.GetDependency<UISystem>();
        _audioSystem = creator.GetDependency<AudioSystem>();
    }
    
    public virtual void Create(AllTagNames uiTag)
    {
        _panel = Creator.GetChildComponentByName<Transform>(_parentCanvas.canvas.gameObject, uiTag);
    }

    public virtual void Show()
    {
        Debug.Log("Showing: "+_panel.name);
        _panel.gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        Debug.Log("Hiding: "+_panel.name);
        _panel.gameObject.SetActive(false);
    }
}
