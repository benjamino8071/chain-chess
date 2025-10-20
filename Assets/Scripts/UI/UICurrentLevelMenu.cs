using System.Collections.Generic;
using Michsky.MUIP;
using UnityEngine;

public class UICurrentLevelMenu : UIPanel
{
    private TurnSystem _turnSystem;

    private ButtonManager _sectionsButton;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);
        
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
        
        _sectionsButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonLevels);
        _sectionsButton.onClick.AddListener(() =>
        {
            if (_parentCanvas.leftBotSidePanelOpen == AllTagNames.UISections)
            {
                _uiSystem.ShowLeftBotSideUI(AllTagNames.UICurrentScore);
                _uiSystem.ShowRightTopSideUI(AllTagNames.UIChain);
                
                _audioSystem.PlayMenuCloseSfx();
            }
            else
            {
                _uiSystem.ShowLeftBotSideUI(AllTagNames.UISections);

                List<UILevels> uiLevelss = _uiSystem.GetUI<UILevels>();
                foreach (UILevels uiLevels in uiLevelss)
                {
                    uiLevels.SetLevels(_turnSystem.currentLevel.section);
                }
                
                _uiSystem.ShowRightTopSideUI(AllTagNames.UILevels);
                
                _audioSystem.PlayMenuOpenSfx();
            }
        });
    }

    public void SetLevelsButtonText(Level level)
    {
        _sectionsButton.SetText($"{level.section} - {level.level}");
    }
}
