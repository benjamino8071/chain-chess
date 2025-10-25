using Michsky.MUIP;
using UnityEngine;

public class UIBasicMenu : UIPanel
{
    private ButtonManager _powerButton;
    
    private bool _powerButtonPressed;

    private const float MaxTime = 3;
    private float _timer;

    public override void Create(AllTagNames uiTag)
    {
        base.Create(uiTag);
        
        _powerButton = Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonPower);
        _powerButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            if (_powerButtonPressed)
            {
                Application.Quit(); //Will call OnApplicationQuit which saves to disk
            }
            else
            {
                _powerButtonPressed = true;
                _powerButton.SetIcon(Creator.miscUiSo.tickSprite);
                _timer = MaxTime;
            }
        });
        
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

    public override void GameUpdate(float dt)
    {
        if (_timer > 0)
        {
            _timer -= dt;
            if (_timer <= 0)
            {
                _powerButtonPressed = false;
                _powerButton.SetIcon(Creator.miscUiSo.powerSprite);
            }
        }
    }
}
