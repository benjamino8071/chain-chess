using Michsky.MUIP;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUISystem : Dependency
{
    private AudioSystem _audioSystem;
    private LevelCompleteUISystem _levelCompleteUISystem;
    private LevelSelectUISystem _levelSelectUISystem;
    private GameOverUISystem _gameOverUISystem;
    private TurnSystem _turnSystem;
    private BoardSystem _boardSystem;

    public bool IsShowing => _settingsGui.gameObject.activeSelf;
    
    private Transform _settingsGui;

    private Button _uiBottomResetButton;
    private Button _settingsButton;

    private PieceController _pieceControllerSelected;
    
    private bool _canShow;
    
    public override void GameStart(Creator creator)
    {
        base.GameStart(creator);

        _audioSystem = creator.GetDependency<AudioSystem>();
        _levelCompleteUISystem = creator.GetDependency<LevelCompleteUISystem>();
        _levelSelectUISystem = creator.GetDependency<LevelSelectUISystem>();
        _gameOverUISystem = creator.GetDependency<GameOverUISystem>();
        _turnSystem = creator.GetDependency<TurnSystem>();
        _boardSystem = creator.GetDependency<BoardSystem>();

        _settingsGui = creator.GetFirstObjectWithName(AllTagNames.Settings).transform;
        
        _settingsButton = creator.GetFirstObjectWithName(AllTagNames.SettingsButton).GetComponent<Button>();
        _settingsButton.onClick.AddListener(() =>
        {
            _levelSelectUISystem.Hide();
            
            if (!_settingsGui.gameObject.activeSelf 
                && !_levelCompleteUISystem.IsShowing 
                && !_gameOverUISystem.IsShowing)
            {
                if (_boardSystem.activeSideSystem.pieceControllerSelected is { } pieceControllerSelected)
                {
                    _pieceControllerSelected = pieceControllerSelected;
                    pieceControllerSelected.SetState(PieceController.States.Paused);
                }
                Show();
                _audioSystem.PlayPauseOpenSfx();
            }
            else
            {
                Hide();
                
                if (_pieceControllerSelected != null)
                {
                    _pieceControllerSelected.SetState(PieceController.States.FindingMove);
                    _pieceControllerSelected = null;
                }
                _audioSystem.PlayPauseCloseSfx();
            }
        });

        Transform uiBottom = creator.GetFirstObjectWithName(AllTagNames.GUIBottom);
        Transform uiBottomResetButtonTf = creator.GetChildObjectByName(uiBottom.gameObject, AllTagNames.ResetButton);
        _uiBottomResetButton = uiBottomResetButtonTf.GetComponent<Button>();
        _uiBottomResetButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform resetButtonTf = creator.GetChildObjectByName(_settingsGui.gameObject, AllTagNames.ResetButton);
        Button pauseUiResetButton = resetButtonTf.GetComponent<Button>();
        pauseUiResetButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform exitButtonTf = creator.GetChildObjectByName(_settingsGui.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            SceneManager.LoadScene("Main_Menu_Scene");
        });

        Transform doubleTapSwitchTf = creator.GetChildObjectByName(_settingsGui.gameObject, AllTagNames.DoubleTapSwitch);
        SwitchManager doubleTapSwitch = doubleTapSwitchTf.GetComponent<SwitchManager>();
        if (creator.settingsSo.doubleTap)
        {
            doubleTapSwitch.SetOn();
        }
        else
        {
            doubleTapSwitch.SetOff();
        }
        doubleTapSwitch.onValueChanged.AddListener((isOn) =>
        {
            creator.settingsSo.doubleTap = isOn;
        });
        
        Transform audioSwitchTf = creator.GetChildObjectByName(_settingsGui.gameObject, AllTagNames.AudioSwitch);
        SwitchManager audioSwitch = audioSwitchTf.GetComponent<SwitchManager>();
        if (creator.settingsSo.sound)
        {
            audioSwitch.SetOn();
        }
        else
        {
            audioSwitch.SetOff();
        }
        audioSwitch.onValueChanged.AddListener((isOn) =>
        {
            Creator.settingsSo.sound = isOn;
            UpdateSoundSetting();
        });

        Transform fovSliderTf = creator.GetChildObjectByName(_settingsGui.gameObject, AllTagNames.FOVSlider);
        SliderManager fovSlider = fovSliderTf.GetComponent<SliderManager>();
        fovSlider.sliderEvent.AddListener((value) =>
        {
            Creator.mainCam.fieldOfView = value;
        });
        
        UpdateSoundSetting();
        
        Hide();
    }

    private void UpdateSoundSetting()
    {
        MMSoundManager.Instance.SetVolumeMaster(Creator.settingsSo.sound ? 1 : 0);
    }

    public void ShowButton()
    {
        _settingsButton.gameObject.SetActive(true);
        _uiBottomResetButton.gameObject.SetActive(true);
    }

    public void HideButton()
    {
        _settingsButton.gameObject.SetActive(false);
        _uiBottomResetButton.gameObject.SetActive(false);
    }

    public void Show()
    {
        _settingsGui.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _settingsGui.gameObject.SetActive(false);
    }
}
