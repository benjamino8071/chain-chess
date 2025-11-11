using Michsky.MUIP;
using UnityEngine;

public class UIBasicMenu : UIPanel
{
    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        ButtonManager rulebookButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonRulebook);
        rulebookButton.onClick.AddListener(() =>
        {
            if (_parentCanvas.leftBotSidePanelOpen == AllTagNames.UIRulebook)
            {
                _uiSystem.ShowLeftBotSideUI(AllTagNames.UICurrentLevel);
                
                _audioSystem.PlayMenuCloseSfx();
            }
            else
            {
                _uiSystem.ShowLeftBotSideUI(AllTagNames.UIRulebook);

                _audioSystem.PlayMenuOpenSfx();
            }
        });
        
        ButtonManager settingsButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonSettings);
        settingsButton.onClick.AddListener(() =>
        {
            if (_parentCanvas.leftBotSidePanelOpen == AllTagNames.UISettings)
            {
                _uiSystem.ShowLeftBotSideUI(AllTagNames.UICurrentLevel);
                
                _audioSystem.PlayMenuCloseSfx();
            }
            else
            {
                _uiSystem.ShowLeftBotSideUI(AllTagNames.UISettings);
                
                _audioSystem.PlayMenuOpenSfx();
            }
        });
    }
}
