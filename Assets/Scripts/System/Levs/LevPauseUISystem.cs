using Michsky.MUIP;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevPauseUISystem : LevDependency
{
    private LevAudioSystem _audioSystem;
    private LevLevelCompleteUISystem _levelCompleteUISystem;
    private LevGameOverUISystem _gameOverUISystem;
    
    private Transform _pauseGUI;

    private Button _pauseButton;

    private bool _canShow;
    
    public override void GameStart(LevCreator levCreator)
    {
        base.GameStart(levCreator);

        _audioSystem = levCreator.GetDependency<LevAudioSystem>();
        _levelCompleteUISystem = levCreator.GetDependency<LevLevelCompleteUISystem>();
        _gameOverUISystem = levCreator.GetDependency<LevGameOverUISystem>();

        _pauseGUI = levCreator.GetFirstObjectWithName(AllTagNames.Pause).transform;
        
        _pauseButton = levCreator.GetFirstObjectWithName(AllTagNames.PauseButton).GetComponent<Button>();
        _pauseButton.onClick.AddListener(() =>
        {
            if (!_pauseGUI.gameObject.activeSelf && !_levelCompleteUISystem.isShowing && !_gameOverUISystem.isShowing)
            {
                _pauseGUI.gameObject.SetActive(true);
                _audioSystem.PlayPauseOpenSfx();
            }
            else
            {
                _pauseGUI.gameObject.SetActive(false);
                _audioSystem.PlayPauseCloseSfx();
            }
        });

        Transform resetButtonTf = levCreator.GetChildObjectByName(_pauseGUI.gameObject, AllTagNames.ResetButton);
        Button resetButton = resetButtonTf.GetComponent<Button>();
        resetButton.onClick.AddListener(() =>
        {
            Creator.playerSystemSo.startingPiece = Piece.NotChosen;
        
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        
        Transform exitButtonTf = levCreator.GetChildObjectByName(_pauseGUI.gameObject, AllTagNames.Exit);
        Button exitButton = exitButtonTf.GetComponent<Button>();
        exitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenuScene");
        });

        Transform doubleTapSwitchTf = levCreator.GetChildObjectByName(_pauseGUI.gameObject, AllTagNames.DoubleTapSwitch);
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
        
        Transform audioSwitchTf = levCreator.GetChildObjectByName(_pauseGUI.gameObject, AllTagNames.AudioSwitch);
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
        
        _pauseGUI.gameObject.SetActive(false);
    }

    private void UpdateSoundSetting()
    {
        MMSoundManager.Instance.SetVolumeMaster(Creator.settingsSo.sound ? 1 : 0);
    }

    public void ShowButton()
    {
        _pauseButton.gameObject.SetActive(true);
    }

    public void HideButton()
    {
        _pauseButton.gameObject.SetActive(false);
    }
}
