using Michsky.MUIP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElSettingsUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    
    private Transform _settingsGUI;
    
    private ButtonManager _settingsButton;

    private ButtonManager _newRunButton;

    private SwitchManager _doubleTapSwitch;
    private SwitchManager _audioSwitch;

    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        _playerSystem = elCreator.GetDependency<ElPlayerSystem>();
        
        _settingsGUI = elCreator.GetFirstObjectWithName(AllTagNames.Settings);

        Transform settingsButtonTf = elCreator.GetFirstObjectWithName(AllTagNames.SettingsButton);

        _settingsButton = settingsButtonTf.GetComponent<ButtonManager>();
        
        _settingsButton.onClick.AddListener(() =>
        {
            if (!_settingsGUI.gameObject.activeSelf)
            {
                Show();
                _playerSystem.SetState(ElPlayerSystem.States.WaitingForTurn);
                if (elCreator.settingsSo.doubleTap)
                {
                    _playerSystem.UnSetPositionRequested();
                }
            }
            else
            {
                Hide();
                _playerSystem.SetState(ElPlayerSystem.States.Idle);
            }
        });

        Transform newRunButtonTf = elCreator.GetChildObjectByName(_settingsGUI.gameObject, AllTagNames.Exit);

        _newRunButton = newRunButtonTf.GetComponent<ButtonManager>();
        _newRunButton.onClick.AddListener(() =>
        {
            if (_newRunButton.buttonText == "Confirm")
            {
                SceneManager.LoadScene("MainMenuScene");
            }
            else
            {
                _newRunButton.SetText("Confirm");
            }
        });

        Transform doubleTapSwitchTf =
            elCreator.GetChildObjectByName(_settingsGUI.gameObject, AllTagNames.DoubleTapSwitch);

        _doubleTapSwitch = doubleTapSwitchTf.GetComponent<SwitchManager>();
        _doubleTapSwitch.onValueChanged.AddListener((isOn) =>
        {
            elCreator.settingsSo.doubleTap = isOn;
        });

        if (elCreator.settingsSo.doubleTap)
        {
            _doubleTapSwitch.SetOn();
        }
        else
        {
            _doubleTapSwitch.SetOff();
        }
        
        Transform audioSwitchTf = elCreator.GetChildObjectByName(_settingsGUI.gameObject, AllTagNames.AudioSwitch);

        _audioSwitch = audioSwitchTf.GetComponent<SwitchManager>();
        _audioSwitch.onValueChanged.AddListener((isOn) =>
        { 
            elCreator.settingsSo.sound = isOn;
        });
        
        if (elCreator.settingsSo.sound)
        {
            _audioSwitch.SetOn();
        }
        else
        {
            _audioSwitch.SetOff();
        }
        
        Hide();
    }

    public override void GameEarlyUpdate(float dt)
    {
        _settingsButton.gameObject.SetActive(_settingsGUI.gameObject.activeSelf || _playerSystem.GetState() == ElPlayerSystem.States.Idle);
    }

    public void Show()
    {
        _settingsGUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _settingsGUI.gameObject.SetActive(false);
        _newRunButton.SetText("New Run");
    }
}
