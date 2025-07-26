using Michsky.MUIP;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevSettingsUISystem : LevDependency
{
    private LevAudioSystem _audioSystem;
    private LevLevelCompleteUISystem _levelCompleteUISystem;
    private LevGameOverUISystem _gameOverUISystem;
    private LevTurnSystem _turnSystem;
    private LevBoardSystem _boardSystem;

    public bool IsShowing => _settingsGui.gameObject.activeSelf;
    
    private Transform _settingsGui;

    private Button _uiBottomResetButton;
    private Button _settingsButton;

    private LevPieceController _pieceControllerSelected;
    
    private bool _canShow;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        _levelCompleteUISystem = levCreator.GetDependency<LevLevelCompleteUISystem>();
        _gameOverUISystem = levCreator.GetDependency<LevGameOverUISystem>();
        _turnSystem = levCreator.GetDependency<LevTurnSystem>();
        _boardSystem = levCreator.GetDependency<LevBoardSystem>();

        _settingsGui = levCreator.GetFirstObjectWithName(AllTagNames.Settings).transform;
        
        _settingsButton = levCreator.GetFirstObjectWithName(AllTagNames.SettingsButton).GetComponent<Button>();
        _settingsButton.onClick.AddListener(() =>
        {
            if (!_settingsGui.gameObject.activeSelf && !_levelCompleteUISystem.isShowing && !_gameOverUISystem.isShowing)
            {
                if (_boardSystem.activeSideSystem.pieceControllerSelected is { } pieceControllerSelected)
                {
                    _pieceControllerSelected = pieceControllerSelected;
                    pieceControllerSelected.SetState(LevPieceController.States.Paused);
                }
                Show();
                _audioSystem.PlayPauseOpenSfx();
            }
            else
            {
                Hide();
                
                if (_pieceControllerSelected != null)
                {
                    _pieceControllerSelected.SetState(LevPieceController.States.FindingMove);
                    _pieceControllerSelected = null;
                }
                _audioSystem.PlayPauseCloseSfx();
            }
        });

        Transform uiBottom = levCreator.GetFirstObjectWithName(AllTagNames.GUIBottom);
        Transform uiBottomResetButtonTf = levCreator.GetChildObjectByName(uiBottom.gameObject, AllTagNames.ResetButton);
        _uiBottomResetButton = uiBottomResetButtonTf.GetComponent<Button>();
        _uiBottomResetButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform resetButtonTf = levCreator.GetChildObjectByName(_settingsGui.gameObject, AllTagNames.ResetButton);
        Button pauseUiResetButton = resetButtonTf.GetComponent<Button>();
        pauseUiResetButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            
            Hide();
            
            _turnSystem.LoadLevelRuntime();
        });
        
        Transform exitButtonTf = levCreator.GetChildObjectByName(_settingsGui.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            _audioSystem.PlayUIClickSfx();
            SceneManager.LoadScene("MainMenuScene");
        });

        Transform doubleTapSwitchTf = levCreator.GetChildObjectByName(_settingsGui.gameObject, AllTagNames.DoubleTapSwitch);
        SwitchManager doubleTapSwitch = doubleTapSwitchTf.GetComponent<SwitchManager>();
        if (levCreator.settingsSo.doubleTap)
        {
            doubleTapSwitch.SetOn();
        }
        else
        {
            doubleTapSwitch.SetOff();
        }
        
        doubleTapSwitch.onValueChanged.AddListener((isOn) =>
        {
            levCreator.settingsSo.doubleTap = isOn;
        });
        
        Transform audioSwitchTf = levCreator.GetChildObjectByName(_settingsGui.gameObject, AllTagNames.AudioSwitch);
        SwitchManager audioSwitch = audioSwitchTf.GetComponent<SwitchManager>();
        if (levCreator.settingsSo.sound)
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

    private void Show()
    {
        _settingsGui.gameObject.SetActive(true);
    }

    private void Hide()
    {
        _settingsGui.gameObject.SetActive(false);
    }
}
