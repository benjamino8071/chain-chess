using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElPauseUISystem : ElDependency
{
    private ElPlayerSystem _playerSystem;
    
    private Transform _pauseGUI;

    private Button _pauseButton;

    private bool _canShow;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);

        if (elCreator.NewTryGetDependency(out ElPlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }

        _pauseGUI = GameObject.FindWithTag("Pause").transform;

        Button[] buttons = _pauseGUI.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.CompareTag("ResetButton"))
            {
                button.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
            }
            else if (button.CompareTag("Exit"))
            {
                button.onClick.AddListener(() =>
                {
                    Creator.enemySo.ResetCachedSpawnPoints();
                    Creator.playerSystemSo.levelNumberSaved = 0;
                    Creator.playerSystemSo.roomNumberSaved = 0;
                    Creator.timerSo.currentTime = Creator.timerSo.maxTime;
                    
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
            }
        }
        
        _pauseButton = GameObject.FindWithTag("PauseButton").GetComponent<Button>();
        _pauseButton.onClick.AddListener(() =>
        {
            if (!_pauseGUI.gameObject.activeSelf)
            {
                Show();
                _playerSystem.SetState(ElPlayerSystem.States.WaitingForTurn);
            }
            else
            {
                Hide();
                _playerSystem.SetState(ElPlayerSystem.States.Idle);
            }
        });
        
        Hide();
    }

    public override void GameEarlyUpdate(float dt)
    {
        _pauseButton.gameObject.SetActive(_pauseGUI.gameObject.activeSelf || _playerSystem.GetState() == ElPlayerSystem.States.Idle);
    }

    public void Show()
    {
        _pauseGUI.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _pauseGUI.gameObject.SetActive(false);
    }
}
