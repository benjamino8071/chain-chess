using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUISystem : ElDependency
{
    private PlayerSystem _playerSystem;
    
    private Transform _pauseGUI;

    private Button _pauseButton;

    private bool _canShow;
    
    public override void GameStart(ElCreator elCreator)
    {
        base.GameStart(elCreator);
        
        if(Creator.TryGetDependency("PlayerSystem", out PlayerSystem playerSystem))
        {
            _playerSystem = playerSystem;
        }

        _pauseGUI = GameObject.FindWithTag("Pause").transform;
        
        _pauseButton = GameObject.FindWithTag("PauseButton").GetComponent<Button>();
        _pauseButton.onClick.AddListener(TogglePause);
        
        _pauseGUI.GetComponentInChildren<Button>().onClick.AddListener(GiveUp);
        
        Hide();
    }

    public override void GameEarlyUpdate(float dt)
    {
        _pauseButton.gameObject.SetActive(_pauseGUI.gameObject.activeSelf || _playerSystem.GetState() == PlayerSystem.States.Idle);
    }

    private void GiveUp()
    {
        Creator.playerSystemSo.startingPiece = Piece.NotChosen;
        Creator.playerSystemSo.roomNumberSaved = 0;
        Creator.timerSo.currentTime = Creator.timerSo.maxTime;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void TogglePause()
    {
        if (!_pauseGUI.gameObject.activeSelf)
        {
            Show();
            _playerSystem.SetState(PlayerSystem.States.WaitingForTurn);
        }
        else
        {
            Hide();
            _playerSystem.SetState(PlayerSystem.States.Idle);
        }
    }

    private void Show()
    {
        _pauseGUI.gameObject.SetActive(true);

    }

    private void Hide()
    {
        _pauseGUI.gameObject.SetActive(false);

    }
}
