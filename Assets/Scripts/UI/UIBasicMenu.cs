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
            if (_powerButtonPressed)
            {
                Application.Quit();
                Debug.Log("APPLICATION QUIT");
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
            if (_uiSystem.leftSidePanelOpen == AllTagNames.UIRulebook)
            {
                _uiSystem.HideLeftSideUI();
            }
            else
            {
                _uiSystem.ShowLeftSideUI(AllTagNames.UIRulebook);
            }
        });
        
        ButtonManager settingsButton =
            Creator.GetChildComponentByName<ButtonManager>(_panel.gameObject, AllTagNames.ButtonSettings);
        settingsButton.onClick.AddListener(() =>
        {
            if (_uiSystem.leftSidePanelOpen == AllTagNames.UISettings)
            {
                _uiSystem.HideLeftSideUI();
            }
            else
            {
                _uiSystem.ShowLeftSideUI(AllTagNames.UISettings);
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
