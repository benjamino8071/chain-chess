using Michsky.MUIP;
using UnityEngine;

public class UICurrentLevelMenu : UIPanel
{
    private AudioSystem _audioSystem;
    private TurnSystem _turnSystem;

    private ButtonManager _levelsButton;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
        _audioSystem = creator.GetDependency<AudioSystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
    }

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);

        ButtonManager resetButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonReset);
        resetButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();

            _turnSystem.ReloadCurrentLevel();
        });
        
        _levelsButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonLevels);
        _levelsButton.onClick.AddListener(() =>
        {
            if (_uiSystem.leftSidePanelOpen == AllTagNames.UISections)
            {
                _uiSystem.HideLeftSideUI();
            }
            else
            {
                _uiSystem.ShowLeftSideUI(AllTagNames.UISections);
            }
        });
    }

    public void SetLevelsButtonText(Level level)
    {
        _levelsButton.SetText($"{level.section} - {level.level}");
    }
}
