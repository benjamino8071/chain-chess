using UnityEngine;

public class UIPanel : Dependency
{
    protected UISystem _uiSystem;
    protected AudioSystem _audioSystem;
    
    public bool IsShowing => _panel.gameObject.activeSelf;
    
    protected Transform _panel;

    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _uiSystem = creator.GetDependency<UISystem>();
        _audioSystem = creator.GetDependency<AudioSystem>();
    }

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
